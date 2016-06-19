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
        private bool isNew = true;
        private bool isModified = false;
        private bool isDeleting = false;
        private BusinessBase owner = null;

        public DataItem(BusinessBase owner, object[] values)
        {
            this.owner = owner;
            this.values = values;
        }

        public bool IsNew
        {
            get
            {
                return isNew;
            }

            set
            {
                isNew = value;
            }
        }

        public bool IsModified
        {
            get
            {
                return isModified;
            }

            set
            {
                isModified = value;
            }
        }

        public bool IsDeleting
        {
            get
            {
                return isDeleting;
            }

            set
            {
                isDeleting = value;
            }
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
                        IsModified = ((IComparable)values[index]).CompareTo(value) != 0;
                    }
                    else
                    {
                        IsModified = !(value == null && values[index] == null);
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

        public string Key
        {
            get
            {
                if (keyIsDirty)
                {
                    SetKey();
                }

                return key;
            }
        }

        public virtual byte[] Serialize()
        {
            //private object[] values;
            //private bool isNew = true;
            //private bool isModified = false;
            //private bool isDeleting = false;
            JObject obj = new JObject();

            obj.Add("isNew", JToken.FromObject(isNew));
            obj.Add("isModified", JToken.FromObject(isModified));
            obj.Add("IsDeleting", JToken.FromObject(IsDeleting));

            foreach (PropertyDefinition prop in owner.Definition.ListProperties)
            {
                int index = prop.Index;
                object value = values[index];

                obj["n" + prop.Index] = JToken.FromObject(value == null);
                obj["v" + prop.Index] = JToken.FromObject(value ?? 0);
            }

            return Encoding.Unicode.GetBytes(obj.ToString(Newtonsoft.Json.Formatting.None));
        }

        public void Deserialize(byte[] data)
        {
            string json = Encoding.Unicode.GetString(data);
            JObject obj = JObject.Parse(json);

            isNew = (bool)obj["isNew"].ToObject(typeof(bool));
            isModified = (bool)obj["isModified"].ToObject(typeof(bool));
            isDeleting = (bool)obj["isDeleting"].ToObject(typeof(bool));
            foreach (PropertyDefinition prop in owner.Definition.ListProperties)
            {
                int index = prop.Index;
                bool isNull = (bool)obj["n" + prop.Index].ToObject(typeof(bool));

                if (isNull)
                {
                    values[index] = null;
                }
                else
                {
                    values[index] = obj["n" + prop.Index].ToObject(prop.DataType);
                }
            }

            keyIsDirty = true;
        }

        public bool Validate(ref string LastErrorMessage, ref string LastErrorProperty)
        {
            bool validated = true;

            // Null validations and something more...

            return validated;
        }
    }
}
