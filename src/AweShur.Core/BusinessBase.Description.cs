using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public virtual string Description
        {
            get
            {
                return this[Definition.FirstStringProperty.Index].NoNullString();
            }
        }

        public virtual string Title
        {
            get
            {
                return Definition.Singular + " " + Description + 
                    (IsNew ? " (new)" : 
                        (IsDeleting ? " (deleting)" : 
                            (IsModified ? " (modified)" : "")));
            }
        }

        public virtual bool IsReadOnly(string propertyName)
        {
            return Definition.Properties[propertyName].IsReadOnly;
        }

        public virtual bool IsNew
        {
            get
            {
                return dataItem.IsNew;
            }
            set
            {
                dataItem.IsNew = value;
            }
        }

        public virtual bool IsModified
        {
            get
            {
                bool mod = dataItem.IsModified;

                if (!mod)
                {
                    foreach (BusinessCollectionBase col in relatedCollections.Values)
                    {
                        if (col.IsModified)
                        {
                            mod = true;
                            break;
                        }
                    }
                }

                return mod;
            }
            set
            {
                dataItem.IsModified = value;
            }
        }

        public virtual bool IsDeleting
        {
            get
            {
                return dataItem.IsDeleting;
            }
            set
            {
                if (IsNew && value)
                {
                    throw new Exception("New object, can't mark as deleting.");
                }
                dataItem.IsDeleting = value;
            }
        }

        public virtual bool CanDelete
        {
            get
            {
                return !(IsNew || IsDeleting);
            }
        }

        public virtual bool IsNewOrChanged
        {
            get
            {
                return IsModified || IsNew || IsDeleting;
            }
        }

        public virtual string Value(string valueName)
        {
            if (valueName == "Description")
            {
                return this.Description;
            }

            return "";
        }
    }
}
