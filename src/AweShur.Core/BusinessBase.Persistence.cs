using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public void SetNew(bool preserve = false, bool withoutCollections = false)
        {
            if (!preserve)
            {
                dataItem = Definition.New(this);
            }
            IsNew = true;

            if (!withoutCollections)
            {
                foreach (BusinessCollectionBase col in relatedCollections.Values)
                {
                    col.SetNew(preserve, withoutCollections);
                }
            }
            PostSetNew();
        }

        public virtual bool ReadFromDB(string key)
        {
            string[] keys = DataItem.SplitKey(key);

            if (key.Length != Definition.PrimaryKeys.Count)
            {
                throw new Exception("Invalid key (" + key + ") for object " + Description);
            }

            foreach (int index in Definition.PrimaryKeys)
            {
                Definition.ListProperties[index].SetValue(this, keys[index]);
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

            if (!IsNew)
            {
                try
                {
                    CurrentDB.ReadBusinessObject(this);

                    IsNew = false;
                    IsModified = false;
                    IsDeleting = false;

                    AfterReadFromDB();
                }
                catch(Exception exp)
                {
                    readed = false;
                }
            }

            return readed;
        }

        public virtual void AfterReadFromDB()
        {
            foreach (BusinessCollectionBase c in relatedCollections.Values)
            {
                c.Reset();
            }
        }

        public virtual void StoreToDB()
        {
            LastErrorMessage = "";
            LastErrorProperty = "";

            if (IsDeleting || IsNew || IsModified)
            {
                bool isValidated = IsDeleting ? true : Validate();

                if (isValidated)
                {
                    if (BeforeStoreToDB())
                    {
                        bool wasNew = IsNew;
                        bool wasModified = IsModified;
                        bool wasDeleting = IsDeleting;

                        if (IsDeleting)
                        {
                            foreach (BusinessCollectionBase col in relatedCollections.Values)
                            {
                                col.SetForDeletion();
                                col.StoreToDB();
                            }
                        }

                        CurrentDB.StoreBusinessObject(this);

                        foreach (BusinessCollectionBase col in relatedCollections.Values)
                        {
                            if (col.MustSave)
                            {
                                col.StoreToDB();
                            }
                        }

                        IsNew = false;
                        IsModified = false;
                        IsDeleting = false;

                        BusinessBaseProvider.ListProvider.Invalidate(ObjectName);

                        AfterStoreToDB(wasNew, wasModified, wasDeleting);
                    }
                }
                else
                {
                    throw new Exception(LastErrorMessage);
                }
            }
        }

        protected virtual bool BeforeStoreToDB()
        {
            return true;
        }

        protected virtual void AfterStoreToDB(bool wasNew, bool wasModified, bool wasDeleting)
        {
        }

        public virtual void SetPropertiesFrom(BusinessBase source)
        {
        }

        public virtual void PostSetNew()
        {
        }

        public virtual void CopyTo(BusinessBase Target, List<string> excludeFieldNames)
        {
            foreach (PropertyDefinition prop in Definition.ListProperties)
            {
                if (!prop.IsReadOnly
                    && !prop.IsPrimaryKey
                    && (excludeFieldNames == null || (excludeFieldNames != null
                    && !excludeFieldNames.Contains(prop.FieldName))))
                {
                    Target[prop.FieldName] = this[prop.FieldName];
                }
            }
        }
    }
}
