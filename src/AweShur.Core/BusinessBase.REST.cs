using AweShur.Core.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public virtual ModelToClient ToClient(ModelFromClient fromClient)
        {
            ModelToClient model = new ModelToClient();

            model.data = new Dictionary<string, string>(Definition.ListProperties.Count);

            foreach(PropertyDefinition prop in Definition.ListProperties)
            {
                if (fromClient.dataNames.Contains(prop.FieldName))
                {
                    model.data.Add(prop.FieldName, prop.GetValue(this));
                }
            }

            return model;
        }
    }
}
