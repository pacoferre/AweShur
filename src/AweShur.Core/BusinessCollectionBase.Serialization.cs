using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessCollectionBase
    {
        public virtual JObject ToJObject()
        {
            JObject obj = new JObject();

            obj.Add("r", JToken.FromObject(readed ? "1" : ""));

            if (readed)
            {
                int index = 0;

                obj.Add("c", JToken.FromObject(list.Count));

                foreach(BusinessBase item in list)
                {
                    obj.Add("i" + index, item.ToJObject());
                    index++;
                }
            }

            return obj;
        }

        public virtual void FromJObject(JObject obj)
        {
            readed = obj["r"].ToObject(typeof(string)).ToString() == "1";

            if (readed)
            {
                int count = (int)obj["c"].ToObject(typeof(int));

                for (int index = 0; index < count; ++index)
                {
                    BusinessBase item = CreateNewChild();

                    item.FromJObject((JObject)obj["i" + index]);
                    item.Parent = this;

                    list.Add(item);
                }
            }
        }

    }
}
