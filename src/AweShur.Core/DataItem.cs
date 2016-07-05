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
                            IsModified = true;
                        }
                    }
                    else
                    {
                        if (!(value == null && values[index] == null))
                        {
                            IsModified = true;
                        }
                    }
                }

                values[index] = value;
                if (owner.Definition.PrimaryKeys.Contains(index))
                {
                    keyIsDirty = true;
                }
            }
        }

        private void SetKey()
        {
            lock (key)
            {
                key = "";

                foreach (int index in owner.Definition.PrimaryKeys)
                {
                    key += values[index].NoNullString() + "_";
                }

                key = key.TrimEnd('_');
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

                return IsNew ? "0" : key;
            }
        }

        public byte[] Serialize()
        {
            JObject obj = new JObject();

            obj.Add("IsNew", JToken.FromObject(IsNew ? "1" : ""));
            obj.Add("IsModified", JToken.FromObject(IsModified ? "1" : ""));
            obj.Add("IsDeleting", JToken.FromObject(IsDeleting ? "1" : ""));

            foreach (PropertyDefinition prop in owner.Definition.ListProperties)
            {
                int index = prop.Index;
                object value = values[index];

                obj["n" + prop.Index] = JToken.FromObject(value == null ? "1" : "");
                if (prop.BasicType == BasicType.Bit)
                {
                    obj["v" + prop.Index] = JToken.FromObject(value.NoNullBool() ? "1" : "");
                }
                else
                {
                    obj["v" + prop.Index] = JToken.FromObject(value ?? "");
                }
            }

            return Encoding.Unicode.GetBytes(obj.ToString(Newtonsoft.Json.Formatting.None));
        }

        public void Deserialize(byte[] data)
        {
            string json = Encoding.Unicode.GetString(data);
            JObject obj = JObject.Parse(json);

            IsNew = obj["IsNew"].ToObject(typeof(string)).ToString() == "1";
            IsModified = obj["IsModified"].ToObject(typeof(string)).ToString() == "1";
            IsDeleting = obj["IsDeleting"].ToObject(typeof(string)).ToString() == "1";
            foreach (PropertyDefinition prop in owner.Definition.ListProperties)
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

        public bool Validate(ref string lastErrorMessage, ref string lastErrorProperty)
        {
            foreach(PropertyDefinition prop in owner.Definition.ListProperties)
            {
                if (!prop.Validate(values[prop.Index], ref lastErrorMessage, ref lastErrorProperty))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
