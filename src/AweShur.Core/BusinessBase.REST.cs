using AweShur.Core.Lists;
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
                model.refreshAll = true;
            }
            else if (fromClient.action == "new")
            {
                if (!IsNew)
                {
                    // Almost innecesary, done by BusinessBaseProvider.RetreiveObject in CRUDController.
                    SetNew();
                }
            }
            else if (fromClient.action == "delete")
            {
                IsDeleting = true;
            }
            else if (fromClient.action == "changed" || fromClient.action == "ok")
            {
                foreach (KeyValuePair<string, string> item in fromClient.root.data)
                {
                    PropertyDefinition prop = Decorator.Properties[item.Key];

                    prop.SetValue(this, item.Value);
                }

                ProcessCollectionsFromClient(context, fromClient, model);

                if (fromClient.action == "ok")
                {
                    model.refreshAll = true;
                    try
                    {
                        string messageAction = IsDeleting ? "deleted" : (IsNew ? "created" : "saved");

                        CurrentDB.BeginTransaction();

                        StoreToDB();

                        model.normalMessage = Description + " " + messageAction + " successfully.";

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

                model.refreshAll = true;
            }

            // Send object data.
            model.data = new Dictionary<string, string>(Decorator.ListProperties.Count);
            if (fromClient.dataNames != null)
            {
                for (int index = 0; index < fromClient.dataNames.Count; ++index)
                {
                    PropertyDefinition prop = Decorator.Properties[fromClient.dataNames[index]];

                    model.data.Add(prop.FieldName, prop.GetValue(this));
                }
            }
            ProcessCollectionsToClient(context, fromClient, model);

            BusinessBaseProvider.StoreObject(this, fromClient.oname);

            model.keyObject = Key;
            model.isNew = IsNew;
            model.isModified = IsModified;
            model.isDeleting = IsDeleting;

            if (ClientRefreshPending && !model.refreshAll)
            {
                model.refreshAll = true;
            }

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

                        if (clientCol.elements != null && !model.refreshAll)
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
                if (model.refreshAll)
                {
                    model.collections = null;
                }
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
                            PropertyDefinition prop = Decorator.Properties[item.Key];

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
            model.data = new Dictionary<string, string>(Decorator.ListProperties.Count);
            if (fromClient.dataNames != null)
            {
                for (int index = 0; index < fromClient.dataNames.Count; ++index)
                {
                    PropertyDefinition prop = Decorator.Properties[fromClient.dataNames[index]];

                    model.data.Add(prop.FieldName, prop.GetValue(this));
                }
            }

            model.isNew = IsNew;
            model.isModified = IsModified;
            model.isDeleting = IsDeleting;

            if (clientRefreshPending && !model.refreshAll)
            {
                model.refreshAll = true;
            }

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
