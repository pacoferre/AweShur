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
    }
}
