﻿using AweShur.Core.Lists;
using AweShur.Core.REST;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public virtual ModelToClient PerformActionAndCreateResponse(HttpContext context, ModelFromClient fromClient)
        {
            ModelToClient model = new ModelToClient();

            model.wasNew = IsNew;
            model.wasDeleting = IsDeleting;
            model.wasModified = IsModified;

            if (fromClient.action == "load")
            {
                ReadFromDB();
            }
            else if (fromClient.action == "new")
            {
                SetNew();
            }
            else if (fromClient.action == "delete")
            {
                IsDeleting = true;
            }
            else if (fromClient.action == "changed" || fromClient.action == "ok")
            {
                foreach (KeyValuePair<string, string> item in fromClient.root.data)
                {
                    PropertyDefinition prop = Definition.Properties[item.Key];

                    prop.SetValue(this, item.Value);
                }

                ProcessCollectionsFromClient(context, fromClient, model);

                if (fromClient.action == "ok")
                {
                    try
                    {
                        CurrentDB.BeginTransaction();

                        StoreToDB();

                        model.normalMessage = Description + " saved successfully.";

                        CurrentDB.CommitTransaction();
                    }
                    catch (Exception exp)
                    {
                        CurrentDB.RollBackTransaction();

                        model.ok = false;
                        model.errorMessage = LastErrorMessage == "" ? exp.Message : LastErrorMessage;
                    }
                }
            }
            else if (fromClient.action == "clear")
            {
                if (IsNew)
                {
                    SetNew();
                }
                else
                {
                    if (IsDeleting)
                    {
                        IsDeleting = false;
                    }

                    ReadFromDB();
                }
            }

            // Return lists.
            if (fromClient.listNames != null && fromClient.listNames.Count > 0)
            {
                model.listItems = new Dictionary<string, List<ListItemRest>>(fromClient.listNames.Count);

                foreach (string listName in fromClient.listNames)
                {
                    string[] parts = listName.Split('*');
                    ListTable table = BusinessBaseProvider.ListProvider.GetList(parts[0], parts[1]);

                    model.listItems.Add(listName, table.ToClient.Where(item => item.i != "0").ToList());
                }
            }

            // Send object data.
            model.data = new Dictionary<string, string>(Definition.ListProperties.Count);
            if (fromClient.dataNames != null)
            {
                for (int index = 0; index < fromClient.dataNames.Count; ++index)
                {
                    PropertyDefinition prop = Definition.Properties[fromClient.dataNames[index]];

                    model.data.Add(prop.FieldName, prop.GetValue(this));
                }
            }
            ProcessCollectionsToClient(context, fromClient, model);

            BusinessBaseProvider.StoreObject(this, fromClient.oname);

            model.keyObject = Key;
            model.isNew = IsNew;
            model.isModified = IsModified;
            model.isDeleting = IsDeleting;

            model.ClientRefreshPending = ClientRefreshPending;

            model.title = Title;

            model.action = fromClient.action;

            return model;
        }

        private void ProcessCollectionsFromClient(HttpContext context, ModelFromClient fromClient, ModelToClient model)
        {
            if (fromClient.root.children != null && fromClient.root.children.Count > 0)
            {
                model.collections = new Dictionary<string, List<ModelToClient>>(fromClient.root.children.Count);

                foreach (ModelFromClientCollection clientCol in fromClient.root.children)
                {
                    BusinessCollectionBase col = Collection(clientCol.path);
                    List<ModelToClient> elements = new List<ModelToClient>(col.Count);
                    ModelFromClientData clientElement;

                    foreach (BusinessBase obj in col)
                    {
                        clientElement = null;

                        if (clientCol.elements != null)
                        {
                            clientElement = clientCol.elements.Find(element => obj.Key == element.key);
                        }

                        elements.Add(obj.ProcessRequestInternalElement(context, clientCol, clientElement, fromClient.action));
                    }

                    model.collections.Add(clientCol.path, elements);
                }
            }
        }

        private void ProcessCollectionsToClient(HttpContext context, ModelFromClient fromClient, ModelToClient model)
        {
            if (fromClient.root.children != null && fromClient.root.children.Count > 0)
            {
                if (model.collections == null)
                {
                    ProcessCollectionsFromClient(context, fromClient, model);
                }
                foreach (ModelFromClientCollection clientCol in fromClient.root.children)
                {
                    BusinessCollectionBase col = Collection(clientCol.path);
                    List<ModelToClient> elements = model.collections[clientCol.path];
                    ModelToClient currentModel = null;

                    foreach (BusinessBase obj in col)
                    {
                        currentModel = elements.Find(element => element.keyObject == obj.Key);

                        obj.ProcessResponseInternalElement(context, clientCol, currentModel);
                    }
                }
            }
        }

        public ModelToClient ProcessRequestInternalElement(HttpContext context, ModelFromClientCollection fromClient,
            ModelFromClientData element, string fromClientAction)
        {
            ModelToClient model = new ModelToClient();

            model.wasNew = IsNew;
            model.wasDeleting = IsDeleting;
            model.wasModified = IsModified;
            model.keyObject = Key;

            // Too much copy/paste
            if (element != null)
            {
                if (fromClientAction == "changed" || fromClientAction == "ok")
                {
                    if (element.data != null)
                    {
                        foreach (KeyValuePair<string, string> item in element.data)
                        {
                            PropertyDefinition prop = Definition.Properties[item.Key];

                            prop.SetValue(this, item.Value);
                        }
                    }
                }
            }

            return model;
        }

        private void ProcessResponseInternalElement(HttpContext context, 
            ModelFromClientCollection fromClient,
            ModelToClient model)
        {
            // Send object data.
            model.data = new Dictionary<string, string>(Definition.ListProperties.Count);
            if (fromClient.dataNames != null)
            {
                for (int index = 0; index < fromClient.dataNames.Count; ++index)
                {
                    PropertyDefinition prop = Definition.Properties[fromClient.dataNames[index]];

                    model.data.Add(prop.FieldName, prop.GetValue(this));
                }
            }

            model.isNew = IsNew;
            model.isModified = IsModified;
            model.isDeleting = IsDeleting;

            model.ClientRefreshPending = ClientRefreshPending;

            model.title = Title;

            // First level now...
            //if (relatedCollections.Count > 0)
            //{
            //    model.collections = new Dictionary<string, List<ModelToClient>>(relatedCollections.Count);

            //    foreach (BusinessCollectionBase col in relatedCollections.Values)
            //    {
            //        List<ModelToClient> elements = new List<ModelToClient>(col.Count);

            //        foreach (BusinessBase obj in col)
            //        {
            //            elements.Add(obj.CreateResponse(null, null));
            //        }
            //    }
            //}
        }

        private bool clientRefreshPending = false;
        public virtual bool ClientRefreshPending
        {
            get
            {
                return clientRefreshPending;
            }
            set
            {
                clientRefreshPending = value;
            }
        }
    }
}
