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

        protected BusinessBaseDecorator decorator = null;
        protected DataItem dataItem = null;

        public BusinessBase(string tableName, bool noDB)
        {
            decorator = BusinessBaseProvider.Instance.GetDecorator(tableName, 0);
            dataItem = Decorator.New(this);
        }

        public BusinessBase(string tableName, int dbNumber = 0)
        {
            decorator = BusinessBaseProvider.Instance.GetDecorator(tableName, dbNumber);
            dataItem = Decorator.New(this);

            lazyDB = new Lazy<DB>(() => DB.InstanceNumber(dbNumber));
        }

        public BusinessBaseDecorator Decorator
        {
            get
            {
                return decorator;
            }
        }

        public virtual string ObjectName
        {
            get
            {
                string objectName = ToString();

                if (objectName.Contains("BusinessBase"))
                {
                    return Decorator.ObjectName;
                }

                return objectName.Substring(objectName.LastIndexOf(".") + 1);
            }
        }

        public virtual string GenerateKey(object[] dataItemValues)
        {
            string key = "";

            foreach (int index in Decorator.PrimaryKeys)
            {
                key += dataItemValues[index].NoNullString() + "_";
            }

            return key.TrimEnd('_');
        }

        public string Key
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
                return dataItem[Decorator.IndexOfName(property)];
            }
            set
            {
                dataItem[Decorator.IndexOfName(property)] = value;
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

        public override bool Equals(object obj)
        {
            // Stackoverflow... ups
            //if (obj is BusinessBase)
            //{
            //    return (BusinessBase)obj == this;
            //}

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (ObjectName + "_" + Key).GetHashCode();
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
