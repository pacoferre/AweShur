using System;
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



        public bool IsNew
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

        public bool IsModified
        {
            get
            {
                return dataItem.IsModified;
            }

            set
            {
                dataItem.IsModified = value;
            }
        }

        public bool IsDeleting
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
    }
}
