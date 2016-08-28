using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public virtual void SetNew(bool preserve = false, bool withoutCollections = false)
        {
            if (!preserve)
            {
                dataItem = Decorator.New(this);
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
            if (key.StartsWith("-"))
            {
                throw new Exception("Invalid key (" + key + ") for object " + ObjectName
                    + ". Minus char is for new object");
            }

            string[] keys = DataItem.SplitKey(key);

            if (keys.Length != Decorator.PrimaryKeys.Count)
            {
                throw new Exception("Invalid key (" + key + ") for object " + ObjectName);
            }

            foreach (int index in Decorator.PrimaryKeys)
            {
                Decorator.ListProperties[index].SetValue(this, keys[index]);
            }

            return ReadFromDB();
        }

        public virtual bool ReadFromDB(int key)
        {
            if (!Decorator.primaryKeyIsOneInt)
            {
                throw new Exception("Primary key is not int.");
            }

            this[Decorator.PrimaryKeys[0]] = key;

            return ReadFromDB();
        }

        public virtual bool ReadFromDB(long key)
        {
            if (!Decorator.primaryKeyIsOneLong)
            {
                throw new Exception("Primary key is not long.");
            }

            this[Decorator.PrimaryKeys[0]] = key;

            return ReadFromDB();
        }

        public virtual bool ReadFromDB(Guid key)
        {
            if (!Decorator.primaryKeyIsOneGuid)
            {
                throw new Exception("Primary key is not guid.");
            }

            this[Decorator.PrimaryKeys[0]] = key;

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
                catch
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
            foreach (PropertyDefinition prop in Decorator.ListProperties)
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
