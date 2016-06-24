using AweShur.Core.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public virtual ModelToClient ToClient()
        {
            ModelToClient model = new ModelToClient();

            model.data = new Dictionary<string, string>(Definition.ListProperties.Count);

            foreach(PropertyDefinition prop in Definition.ListProperties)
            {
                model.data.Add(prop.PropertyName, prop.GetValue(this));
            }

            return model;
        }
    }
}
