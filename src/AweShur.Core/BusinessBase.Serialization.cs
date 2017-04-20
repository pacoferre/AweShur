using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public virtual byte[] Serialize()
        {
            JObject obj = ToJObject();

            return Encoding.Unicode.GetBytes(obj.ToString(Newtonsoft.Json.Formatting.None));
        }

        public virtual JObject ToJObject()
        {
            JObject obj = new JObject();

            obj.Add("t", dataItem.ToJObject());

            foreach (var item in relatedCollections)
            {
                obj.Add("c" + item.Key, item.Value.ToJObject());
            }

            return obj;
        }

        public virtual void Deserialize(byte[] data)
        {
            string json = Encoding.Unicode.GetString(data);
            JObject obj = JObject.Parse(json);

            FromJObject(obj);
        }

        public virtual void FromJObject(JObject obj)
        {
            dataItem.FromJObject((JObject)obj["t"]);

            foreach (var item in relatedCollections)
            {
                item.Value.FromJObject((JObject)obj["c" + item.Key]);
            }
        }
    }
}
