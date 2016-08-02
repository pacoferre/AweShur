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
            }
            else if (fromClient.action == "new")
            {
                SetNew();
            }
            else if (fromClient.action == "delete")
            {
                IsDeleting = true;
            }
            else if (fromClient.action == "changed")
            {
                foreach (KeyValuePair<string, string> item in fromClient.root.changed)
                {
                    PropertyDefinition prop = Definition.Properties[item.Key];

                    prop.SetValue(this, item.Value);
                }
            }
            else if (fromClient.action == "ok")
            {
                for (int index = 0; index < fromClient.dataNames.Count; ++index)
                {
                    PropertyDefinition prop = Definition.Properties[fromClient.dataNames[index]];

                    prop.SetValue(this, fromClient.root.data[index]);
                }

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
                foreach (PropertyDefinition prop in Definition.ListProperties)
                {
                    if (fromClient.dataNames.Contains(prop.FieldName))
                    {
                        model.data.Add(prop.FieldName, prop.GetValue(this));
                    }
                }
            }

            BusinessBaseProvider.StoreObject(this, fromClient.oname);

            model.keyObject = Key;
            model.isNew = IsNew;
            model.isModified = IsModified;
            model.isDeleting = IsDeleting;

            model.ClientRefreshPending = ClientRefreshPending;

            model.title = Title;

            model.action = fromClient.action;

            if (fromClient.root.children != null && fromClient.root.children.Count > 0)
            {
                model.collections = new Dictionary<string, List<ModelToClient>>(fromClient.root.children.Count);

                foreach(ModelFromClientCollection clientCol in fromClient.root.children)
                {
                    BusinessCollectionBase col = Collection(clientCol.path);
                    List<ModelToClient> elements = new List<ModelToClient>(col.Count);

                    foreach(BusinessBase obj in col)
                    {
                        ModelFromClientData clientElement = null;

                        if (clientCol.elements != null)
                        {
                            foreach (ModelFromClientData element in clientCol.elements)
                            {
                                if (obj.Key == element.key)
                                {
                                    clientElement = element;
                                }
                            }
                        }

                        elements.Add(obj.CreateResponse(null, clientCol, clientElement, fromClient.action));
                    }

                    model.collections.Add(clientCol.path, elements);
                }
            }

            return model;
        }

        public virtual ModelToClient CreateResponse(HttpContext context, ModelFromClientCollection fromClient,
            ModelFromClientData element, string fromClientAction)
        {
            ModelToClient model = new ModelToClient();

            model.wasNew = IsNew;
            model.wasDeleting = IsDeleting;
            model.wasModified = IsModified;

            // Too much copy/paste
            if (element != null)
            {
                if (fromClientAction == "changed")
                {
                    foreach (KeyValuePair<string, string> item in element.changed)
                    {
                        PropertyDefinition prop = Definition.Properties[item.Key];

                        prop.SetValue(this, item.Value);
                    }
                }
                else if (fromClientAction == "ok")
                {
                    for (int index = 0; index < fromClient.dataNames.Count; ++index)
                    {
                        PropertyDefinition prop = Definition.Properties[fromClient.dataNames[index]];

                        prop.SetValue(this, element.data[index]);
                    }
                }
            }

            // Send object data.
            model.data = new Dictionary<string, string>(Definition.ListProperties.Count);
            if (fromClient.dataNames != null)
            {
                foreach (PropertyDefinition prop in Definition.ListProperties)
                {
                    if (fromClient.dataNames.Contains(prop.FieldName))
                    {
                        model.data.Add(prop.FieldName, prop.GetValue(this));
                    }
                }
            }

            model.keyObject = Key;
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

            return model;
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
