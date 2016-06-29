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

            // Send object data.
            model.data = new Dictionary<string, string>(Definition.ListProperties.Count);
            foreach (PropertyDefinition prop in Definition.ListProperties)
            {
                if (fromClient.dataNames.Contains(prop.FieldName))
                {
                    model.data.Add(prop.FieldName, prop.GetValue(this));
                }
            }

            BusinessBaseProvider.StoreObject(this, fromClient.oname);

            model.keyObject = Key;
            model.isNew = IsNew;
            model.isModified = IsModified;
            model.isDeleting = IsDeleting;

            model.title = Title;

            model.action = fromClient.action;

            return model;
        }
    }
}
