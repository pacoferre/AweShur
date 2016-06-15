using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public class DataItem
    {
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
                    IsModified = ((IComparable)values[index]).CompareTo(value) != 0;
                }

                values[index] = value;
            }
        }
    }
}
