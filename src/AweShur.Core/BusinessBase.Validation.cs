using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public string LastErrorProperty = "";
        private string lastErrorMessage = "";

        public string LastErrorCollection { get; set; } = "";
        public string LastErrorCollectionKey { get; set; } = "";
        public string LastErrorCollectionProperty { get; set; } = "";

        public string LastErrorMessage
        {
            get
            {
                string errorMessage = lastErrorMessage;

                foreach (BusinessCollectionBase col in relatedCollections.Values)
                {
                    if (col.LastErrorMessage != "")
                    {
                        if (errorMessage != "")
                        {
                            errorMessage += " ";
                        }
                        errorMessage += col.LastErrorMessage;
                    }
                }

                return errorMessage;
            }
            set
            {
                this.lastErrorMessage = value;
                if (value == "")
                {
                    foreach (BusinessCollectionBase col in relatedCollections.Values)
                    {
                        col.LastErrorMessage = "";
                    }
                }
            }
        }

        public virtual bool ValidateDataItem(DataItem dataItem, ref string lastErrorMessage, ref string lastErrorProperty)
        {
            foreach (PropertyDefinition prop in Decorator.ListProperties)
            {
                if (!prop.Validate(dataItem[prop.Index], ref lastErrorMessage, ref lastErrorProperty))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool Validate()
        {
            bool valid;

            LastErrorProperty = "";
            LastErrorMessage = "";

            valid = ValidateDataItem(dataItem, ref lastErrorMessage, ref LastErrorProperty);

            if (valid)
            {
                BusinessCollectionBase col;

                foreach (string collectionName in relatedCollections.Keys)
                {
                    col = NotEnsuredCollection(collectionName);

                    if (!col.Validate())
                    {
                        if (valid)
                        {
                            valid = false;
                            this.LastErrorCollection = collectionName;
                            this.LastErrorCollectionProperty = col.LastErrorProperty;
                            this.LastErrorCollectionKey = col.LastErrorKey;
                        }
                    }
                }

                if (valid)
                {
                    if (Parent != null)
                    {
                        if (this == Parent.ActiveObject)
                        {
                            if (IsNew && !Parent.Contains(this))
                            {
                                if (!Parent.CanAddActiveObject)
                                {
                                    LastErrorMessage = "There is another equivalent {OBJECT_NAME}.".Replace("{OBJECT_NAME}", ObjectName);
                                    valid = false;
                                }
                            }
                        }
                    }
                }
            }

            return valid;
        }
    }
}
