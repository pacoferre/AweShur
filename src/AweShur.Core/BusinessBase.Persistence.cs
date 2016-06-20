using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public virtual bool ReadFromDB(string key)
        {
            string[] keys = key.Split();

            for (int index = 0; index < keys.Length; ++index)
            {
                Definition.ListProperties[Definition.PrimaryKeys[index]].SetValue(this, keys[index]);
            }

            return ReadFromDB();
        }

        public virtual bool ReadFromDB(int key)
        {
            if (!Definition.primaryKeyIsOneInt)
            {
                throw new Exception("Primary key is not int.");
            }

            this[Definition.PrimaryKeys[0]] = key;

            return ReadFromDB();
        }

        public virtual bool ReadFromDB(long key)
        {
            if (!Definition.primaryKeyIsOneLong)
            {
                throw new Exception("Primary key is not long.");
            }

            this[Definition.PrimaryKeys[0]] = key;

            return ReadFromDB();
        }

        public virtual bool ReadFromDB(Guid key)
        {
            if (!Definition.primaryKeyIsOneGuid)
            {
                throw new Exception("Primary key is not guid.");
            }

            this[Definition.PrimaryKeys[0]] = key;

            return ReadFromDB();
        }

        public virtual bool ReadFromDB()
        {
            bool readed = true;

            try
            {
                CurrentDB.ReadBusinessObject(this);

                IsNew = false;
                IsModified = false;
                IsDeleting = false;

                AfterReadFromDB();
            }
            catch
            {
                readed = false;
            }

            return readed;
        }

        protected virtual void AfterReadFromDB()
        {

        }

        public virtual void StoreToDB()
        {
            if (Validate())
            {
                CurrentDB.StoreBusinessObject(this);

                IsNew = false;
                IsModified = false;
                IsDeleting = false;

                AfterReadFromDB();
            }
        }
    }
}
