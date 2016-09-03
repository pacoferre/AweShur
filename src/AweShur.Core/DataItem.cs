using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public class DataItem
    {
        private bool keyIsDirty = true;
        private string key = "";
        private string newKey = "";

        private object[] values;
        public bool IsNew { get; set; } = false;
        public bool IsModified { get; set; } = false;
        public bool IsDeleting { get; set; } = false;
        private BusinessBase owner = null;

        public DataItem(BusinessBase owner, object[] values)
        {
            this.owner = owner;
            this.values = values;
        }

        public object this[int index]
        {
            get
            {
                return values[index];
            }
            set
            {
                if (!IsModified)
                {
                    if (value != null && values[index] != null)
                    {
                        if (((IComparable)values[index]).CompareTo(value) != 0)
                        {
                            IsModified = owner.Decorator.setModified[index];
                        }
                    }
                    else
                    {
                        if (!(value == null && values[index] == null))
                        {
                            IsModified = owner.Decorator.setModified[index];
                        }
                    }
                }

                values[index] = value;
                if (owner.Decorator.PrimaryKeys.Contains(index))
                {
                    keyIsDirty = true;
                }
            }
        }

        private void SetKey()
        {
            lock (key)
            {
                key = owner.GenerateKey(values);
            }
        }

        public static string[] SplitKey(string key)
        {
            return key.Split('_');
        }

        public string Key
        {
            get
            {
                if (keyIsDirty)
                {
                    SetKey();
                }

                return IsNew ? GetNewKey() : key;
            }
        }

        private string GetNewKey()
        {
            if (newKey == "")
            {
                lock (newKey)
                {
                    if (newKey == "")
                    {
                        int k = Guid.NewGuid().GetHashCode();

                        newKey = (k > 0 ? -k : k).ToString();
                    }
                }
            }

            return newKey;
        }

        public byte[] Serialize()
        {
            JObject obj = ToJObject();

            return Encoding.Unicode.GetBytes(obj.ToString(Newtonsoft.Json.Formatting.None));
        }

        public JObject ToJObject()
        {
            JObject obj = new JObject();

            obj.Add("N", JToken.FromObject(IsNew ? "1" : ""));
            if (IsNew)
            {
                obj.Add("K", JToken.FromObject(newKey));
            }
            obj.Add("M", JToken.FromObject(IsModified ? "1" : ""));
            obj.Add("D", JToken.FromObject(IsDeleting ? "1" : ""));

            foreach (PropertyDefinition prop in owner.Decorator.ListProperties)
            {
                int index = prop.Index;
                object value = values[index];

                obj["n" + prop.Index] = JToken.FromObject(value == null ? "1" : "");
                if (value != null)
                {
                    if (prop.BasicType == BasicType.Bit)
                    {
                        obj["v" + prop.Index] = JToken.FromObject(value.NoNullBool() ? "1" : "");
                    }
                    else
                    {
                        obj["v" + prop.Index] = JToken.FromObject(value ?? "");
                    }
                }
            }

            return obj;
        }

        public void Deserialize(byte[] data)
        {
            string json = Encoding.Unicode.GetString(data);
            JObject obj = JObject.Parse(json);

            FromJObject(obj);
        }

        public void FromJObject(JObject obj)
        {
            IsNew = obj["N"].ToObject(typeof(string)).ToString() == "1";
            if (IsNew)
            {
                newKey = obj["K"].ToObject(typeof(string)).ToString();
            }
            IsModified = obj["M"].ToObject(typeof(string)).ToString() == "1";
            IsDeleting = obj["D"].ToObject(typeof(string)).ToString() == "1";
            foreach (PropertyDefinition prop in owner.Decorator.ListProperties)
            {
                int index = prop.Index;
                bool isNull = obj["n" + prop.Index].ToObject(typeof(string)).ToString() == "1";

                if (isNull)
                {
                    values[index] = null;
                }
                else
                {
                    if (prop.BasicType == BasicType.Bit)
                    {
                        values[index] = obj["v" + prop.Index].ToObject(typeof(string)).ToString() == "1";
                    }
                    else
                    {
                        values[index] = obj["v" + prop.Index].ToObject(prop.DataType);
                    }
                }
            }

            keyIsDirty = true;
        }
    }
}
