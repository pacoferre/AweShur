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

            model.data = new Dictionary<string, string>(Definition.ListProperties.Count);

            if (fromClient.action == "load")
            {
                ReadFromDB();
            }
            else if (fromClient.action == "ok")
            {
                for (int index = 0; index < fromClient.dataNames.Count; ++index)
                {
                    try
                    {
                        PropertyDefinition prop = Definition.Properties[fromClient.dataNames[index]];

                        prop.SetValue(this, fromClient.root.data[index]);
                    }
                    catch(Exception exc)
                    {
                        int r = 2;
                    }
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

            // Send object data.
            foreach(PropertyDefinition prop in Definition.ListProperties)
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

            return model;
        }
    }
}
