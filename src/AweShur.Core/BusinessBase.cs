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

        public readonly string TableName = "";
        public readonly int DBNumber = 0;

        public BusinessBaseDefinition Definition { get; }
        private DataItem dataItem = null;

        public BusinessBase(string tableName, int dbNumber = 0)
        {
            TableName = tableName;
            DBNumber = dbNumber;
            Definition = BusinessBaseProvider.Instance.GetDefinition(this);
            dataItem = Definition.New(this);

            lazyDB = new Lazy<DB>(() => DB.InstanceNumber(DBNumber));
        }

        public virtual string Key
        {
            get
            {
                return dataItem.Key;
            }
        }

        #region Validation & Status & Indexer
        public string LastErrorProperty = "";
        public string LastErrorMessage = "";
        
        public virtual bool Validate()
        {
            LastErrorProperty = "";
            LastErrorMessage = "";

            return dataItem.Validate(ref LastErrorMessage, ref LastErrorProperty);
        }


        public virtual bool IsReadOnly(string property)
        {
            return false;
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
                dataItem.IsDeleting = value;
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
        #endregion

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
    }
}
