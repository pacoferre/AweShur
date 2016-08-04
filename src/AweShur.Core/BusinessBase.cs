using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AweShur.Core.Security;
using System.Data;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        private Lazy<DB> lazyDB;

        protected BusinessBaseDefinition definition = null;
        protected DataItem dataItem = null;

        public BusinessBase(string tableName, bool noDB)
        {
            definition = BusinessBaseProvider.Instance.GetDefinition(tableName, 0);
            dataItem = Definition.New(this);
        }

        public BusinessBase(string tableName, int dbNumber = 0)
        {
            definition = BusinessBaseProvider.Instance.GetDefinition(tableName, dbNumber);
            dataItem = Definition.New(this);

            lazyDB = new Lazy<DB>(() => DB.InstanceNumber(dbNumber));
        }

        public BusinessBaseDefinition Definition
        {
            get
            {
                return definition;
            }
        }

        public virtual string ObjectName
        {
            get
            {
                string objectName = ToString();

                if (objectName.Contains("BusinessBase"))
                {
                    return Definition.ObjectName;
                }

                return objectName.Substring(objectName.LastIndexOf(".") + 1);
            }
        }

        public virtual string Key
        {
            get
            {
                return dataItem.Key;
            }
        }

        public virtual object this[string property]
        {
            get
            {
                return dataItem[Definition.IndexOfName(property)];
            }
            set
            {
                dataItem[Definition.IndexOfName(property)] = value;
            }
        }

        public object this[int index]
        {
            get
            {
                return dataItem[index];
            }
            set
            {
                dataItem[index] = value;
            }
        }

        protected DB CurrentDB
        {
            get
            {
                return lazyDB.Value;
            }
        }

        private AppUser _currentUser = null;
        public AppUser CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    _currentUser = AppUser.GetAppUserWithoutHttpContext();
                }

                return _currentUser;
            }
        }

        public static bool operator ==(BusinessBase b1, BusinessBase b2)
        {
            if ((object)b1 == null && (object)b2 == null)
            {
                return true;
            }
            if ((object)b1 == null || (object)b2 == null)
            {
                return false;
            }
            if (b1.Key != b2.Key)
            {
                return false;
            }
            return true;
        }

        public static bool operator !=(BusinessBase b1, BusinessBase b2)
        {
            return !(b1 == b2);
        }
    }
}
