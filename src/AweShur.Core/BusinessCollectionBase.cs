using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessCollectionBase : IList<BusinessBase>
    {
        protected int dbNumber = 0;
        private string parentRelationFieldName;
        protected string childRelationFieldName;
        protected string childObjectName;
        private BusinessBase active;
        public BusinessBase Parent { get; set; }
        private string sql = "";
        protected bool readed = false;
        protected string lastErrorKey = "";
        protected string lastErrorProperty = "";
        protected bool alwaysMustSave = false;
        private List<BusinessBase> list = new List<BusinessBase>();

        public BusinessCollectionBase(BusinessBase parent, string parentRelationFieldName, 
            string childObjectName, string sql, int dbNumber = 0)
        {
            this.dbNumber = dbNumber;
            this.Parent = parent;
            this.parentRelationFieldName = parentRelationFieldName;
            this.childObjectName = childObjectName;
            this.childRelationFieldName = BusinessBaseProvider.Instance.GetDefinition(childObjectName, dbNumber).ListProperties[0].FieldName;
            this.sql = sql;
        }

        private string collectionName = "";

        public virtual string CollectionName
        {
            set
            {
                collectionName = value;
            }
            get
            {
                if (collectionName == "")
                {
                    string nom;

                    nom = this.ToString();
                    collectionName = nom.Substring(nom.LastIndexOf(".") + 1);
                }

                return collectionName;
            }
        }

        public void EnsureList()
        {
            if (!readed)
            {
                ReadFromDBInternal();
            }
        }

        public void LeaveOnlyNew()
        {
            for (int i = Count - 1; i >= 0; --i)
            {
                if (!this[i].IsNew)
                {
                    this.Remove(this[i]);
                }
            }

            ClientRefreshPending = true;
            Parent.ClientRefreshPending = true;
            // Changed();  Not here.
        }

        public void Reset()
        {
            this.Clear();
            readed = false;
            ClientRefreshPending = true;
            Parent.ClientRefreshPending = true;
            // Changed();  Not here.
        }

        public virtual void Changed()
        {
            ClientRefreshPending = true;
            Parent.ClientRefreshPending = true;
        }

        public string SQL
        {
            get
            {
                return sql;
            }
            set
            {
                if (sql != value)
                {
                    Reset();
                    sql = value;
                }
            }
        }

        public virtual BusinessBase CreateNewChild()
        {
            return BusinessBaseProvider.Instance.CreateObject(childObjectName, dbNumber);
        }

        public BusinessBase CreateNew()
        {
            BusinessBase b;

            b = CreateNewChild();
            b.Parent = this;
            b.SetPropertiesFrom(Parent);

            Add(b);
            ActiveObject = b;

            return b;
        }

        public virtual string SQLQuery
        {
            get
            {
                return sql;
            }
        }

        public object SQLParameters
        {
            get
            {
                if (parentRelationFieldName == "")
                {
                    return null;
                }

                return new { id = Parent[parentRelationFieldName] };
            }
        }

        public int CuantosEnCollection
        {
            get
            {
                EnsureList();

                return this.Count;
            }
        }

        public int CountNoRead
        {
            get
            {
                if (readed)
                {
                    return this.Count;
                }
                else
                {
                    if (SQLQuery.ToUpper().Contains("ORDER BY"))
                    {
                        return DB.Instance.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM (" + SQLQuery.Substring(0, SQLQuery.ToUpper().LastIndexOf("ORDER BY")) + ") As D");
                    }
                    else
                    {
                        return DB.Instance.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM (" + SQLQuery + ") As D");
                    }
                }
            }
        }

        private object lockReadFromDBInternal = new object();

        private void ReadFromDBInternal()
        {
            bool afterReadFromDBPending = false;

            lock (lockReadFromDBInternal)
            {
                if (sql != "" && !readed)
                {
                    readed = true;
                    DB.Instance.ReadBusinessCollection(this);

                    afterReadFromDBPending = true;
                }
            }

            if (afterReadFromDBPending)
            {
                AfterReadFromDB();
            }
        }

        public virtual void AfterReadFromDB()
        {
        }

        public virtual bool Validate()
        {
            bool valid = true;

            lastErrorKey = "";
            lastErrorProperty = "";

            if (readed)
            {
                foreach (BusinessBase b in this)
                {
                    if (!b.Validate())
                    {
                        valid = false;
                        lastErrorKey = b.Key;
                        lastErrorProperty = b.LastErrorProperty;
                        break;
                    }
                }
            }

            return valid;
        }

		public int CountWhenSaving
        {
            get
            {
                int c = 0;

                foreach (BusinessBase b in this)
                {
                    if (!b.IsDeleting)
                    {
                        c++;
                    }
                }

                return c;
            }
        }

        public virtual string Description
        {
            get
            {
                StringBuilder desc = new StringBuilder();

                foreach (BusinessBase b in this)
                {
                    if (b is Specialized.N2M)
                    {
                        if ((bool)b["Active"])
                        {
                            desc.Append(" " + b.Description);
                        }
                    }
                    else
                    {
                        desc.Append(" " + b.Description);
                    }
                }

                return desc.ToString();
            }
        }

        public virtual string LastErrorMessage
        {
            get
            {
                string errorMessage = "";

                if (this.readed)
                {
                    foreach (BusinessBase b in this)
                    {
                        if (b.LastErrorMessage != "")
                        {
                            if (errorMessage != "")
                            {
                                errorMessage += ". ";
                            }
                            errorMessage += b.LastErrorMessage;
                        }
                    }
                }

                return errorMessage;
            }
            set
            {
                if (value == "")
                {
                    if (this.readed)
                    {
                        foreach (BusinessBase b in this)
                        {
                            b.LastErrorMessage = "";
                        }
                    }
                }
                else
                {
                    Parent.LastErrorMessage = value;
                }
            }
        }

        public string LastErrorKey
        {
            get
            {
                return lastErrorKey;
            }
            set
            {
                lastErrorKey = value;
            }
        }
        public string LastErrorProperty
        {
            get
            {
                return lastErrorProperty;
            }
            set
            {
                lastErrorProperty = value;
            }
        }

        public bool MustSave
        {
            get
            {
                if (alwaysMustSave)
                {
                    return true;
                }
                else
                {
                    return readed;
                }
            }
        }

        public virtual void StoreToDB()
        {
            if (!readed && !Parent.IsDeleting)
            {
                return;
            }
            EnsureList();
            foreach (BusinessBase b in this)
            {
                if (parentRelationFieldName != "")
                {
                    if (b[childRelationFieldName] == null)
                    {
                        b[childRelationFieldName] = Parent[parentRelationFieldName];
                    }
                    else
                    {
                        if (Parent.Definition.primaryKeyIsOneInt)
                        {
                            if ((int)b[childRelationFieldName] != (int)Parent[parentRelationFieldName])
                            {
                                b[childRelationFieldName] = Parent[parentRelationFieldName];
                            }
                        }
                        else if (Parent.Definition.primaryKeyIsOneLong)
                        {
                            if ((long)b[childRelationFieldName] != (long)Parent[parentRelationFieldName])
                            {
                                b[childRelationFieldName] = Parent[parentRelationFieldName];
                            }
                        }
                        else
                        {
                            if (((IComparable)b[childRelationFieldName]).CompareTo(Parent[parentRelationFieldName]) != 0)
                            {
                                b[childRelationFieldName] = Parent[parentRelationFieldName];
                            }
                        }
                    }
                }
                b.StoreToDB();
            }
            readed = false;
            // For health (N2M and others).
            if (!Parent.IsDeleting)
            {
                CreateNew();
            }
        }

        /// <summary>
        /// Elimina los registros nuevos de la colección y marca para eliminar los que existen.
        /// </summary>
		public virtual void SetForDeletion()
        {
            EnsureList();
            DeleteNewObjects();
            foreach (BusinessBase b in this)
            {
                if (b.CanDelete)
                {
                    b.IsDeleting = true;
                }
            }
        }

        private void DeleteNewObjects()
        {
            // No se puede usar foreach.
            // Son registros nuevos no guardados.
            for (int c = 0; c < this.Count; ++c)
            {
                BusinessBase b = this[c];
                if (b.IsNew)
                {
                    b = null;
                    RemoveAt(c);
                    --c;
                }
            }

            ClientRefreshPending = true;
            Parent.ClientRefreshPending = true;
        }

        public virtual void SetNew(bool preserve = false, bool withoutCollections = false)
        {
            EnsureList();
            foreach (BusinessBase b in this)
            {
                if (b.IsDeleting && !b.IsNew)
                {
                    b.IsDeleting = false;
                }
                b.SetNew(preserve, withoutCollections);
            }
        }

        public virtual void CopyTo(BusinessCollectionBase colTarget)
        {
            EnsureList();
            foreach (BusinessBase objOrigen in this)
            {
                bool isNew = false;
                BusinessBase objTarget = colTarget.Search(objOrigen);

                if (objTarget == null)
                {
                    isNew = true;
                    objTarget = colTarget.CreateNew();
                }

                objOrigen.CopyTo(objTarget, null);

                if (isNew)
                {
                    colTarget.AddActiveObject();
                }
            }

            colTarget.CreateNew();
        }

        public virtual BusinessBase CreateActiveObjectCopy()
        {
            BusinessBase target = CreateNewChild();

            target.Parent = this;

            ActiveObject.CopyTo(target, null);

            Add(target);
            ActiveObject = target;

            return ActiveObject;
        }

        public virtual void AddActiveObject()
        {
            EnsureList();
            if (!this.Contains(this.ActiveObject))
            {
                if (!CanAddActiveObject)
                {
                    throw new System.Exception("Object cannot be added.");
                }
                this.Add(this.ActiveObject);

                Changed();
            }
        }

        public virtual void InsertarActiveObject(int index)
        {
            EnsureList();
            if (!this.Contains(this.ActiveObject))
            {
                if (!CanAddActiveObject)
                {
                    throw new System.Exception("Object cannot be inserted.");
                }
                this.Insert(index, this.ActiveObject);

                Changed();
            }
        }

        public bool CanAddActiveObject
        {
            get
            {
                if (readed)
                {
                    EnsureList();
                    if (ActiveObject.Definition.PrimaryKeys.Count != 1)
                    {
                        foreach (BusinessBase b in this)
                        {
                            if (ActiveObject == b)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
        }

        public void SetActiveObject(int Position)
        {
            EnsureList();
            ActiveObject = this[Position];
        }

        public void SetActiveObject(string Key)
        {
            SetActiveObject(Key, "");
        }

        public int ActiveObjectPosition(string filterName)
        {
            int indice = -1;

            EnsureList();
            if (filterName == "")
            {
                indice = this.IndexOf(ActiveObject);
            }
            else
            {
                int i = 0;

                foreach (BusinessBase b in Filter(filterName))
                {
                    if ((object)ActiveObject == (object)b)
                    {
                        indice = i;
                        break;
                    }
                    ++i;
                }
            }

            return indice;
        }

        public void SetActiveObject(string Key, string filterName)
        {
            EnsureList();
            foreach (BusinessBase b in Filter(filterName))
            {
                if (b.Key == Key)
                {
                    ActiveObject = b;
                    break;
                }
            }
        }

        public virtual BusinessBase Search(BusinessBase searchObject)
        {
            if (searchObject is Specialized.N2M)
            {
                foreach (BusinessBase b in this)
                {
                    string ownFieldNameM = ((Specialized.N2M)searchObject).ExternalFieldNameM;

                    if ((int)b[ownFieldNameM] == (int)searchObject[ownFieldNameM])
                    {
                        return b;
                    }
                }
            }
            else
            {
                return this[searchObject.Key];
            }

            return null;
        }

        public virtual BusinessBase ActiveObject
        {
            get
            {
                if (active == null)
                {
                    EnsureList();
                }

                if (active == null)  // Multitasking issue...
                {
                    ReadFromDBInternal();
                }

                return active;
            }
            set
            {
                active = value;
                if (active != null)
                {
                    active.Parent = this;
                    if (parentRelationFieldName != "")
                    {
                        if (active[childRelationFieldName] == null)
                        {
                            if (Parent.Definition.primaryKeyIsOneInt)
                            {
                                if ((int)Parent[parentRelationFieldName] > 0)
                                {
                                    active[childRelationFieldName] = Parent[parentRelationFieldName];
                                }
                            }
                            else if (Parent.Definition.primaryKeyIsOneLong)
                            {
                                if ((long)Parent[parentRelationFieldName] > 0)
                                {
                                    active[childRelationFieldName] = Parent[parentRelationFieldName];
                                }
                            }
                            else
                            {
                                if (Parent[parentRelationFieldName] != null)
                                {
                                    active[childRelationFieldName] = Parent[parentRelationFieldName];
                                }
                            }
                        }
                        else
                        {
                            if (Parent.Definition.primaryKeyIsOneInt)
                            {
                                if ((int)active[childRelationFieldName] != (int)Parent[parentRelationFieldName])
                                {
                                    active[childRelationFieldName] = Parent[parentRelationFieldName];
                                }
                            }
                            else if (Parent.Definition.primaryKeyIsOneLong)
                            {
                                if ((long)active[childRelationFieldName] != (long)Parent[parentRelationFieldName])
                                {
                                    active[childRelationFieldName] = Parent[parentRelationFieldName];
                                }
                            }
                            else
                            {
                                if (((IComparable)active[childRelationFieldName]).CompareTo(Parent[parentRelationFieldName]) != 0)
                                {
                                    active[childRelationFieldName] = Parent[parentRelationFieldName];
                                }
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable Filter(string filterName)
        {
            if (filterName == "")
            {
                return this;
            }
            else
            {
                return new BusinessCollectionFiltered (this, filterName);
            }
        }

        public BusinessBase this[string key]
        {
            get
            {
                EnsureList();

                if (key == "0") // Carefully...
                {
                    if (!ActiveObject.IsNew)
                    {
                        this.CreateNew();
                    }

                    return ActiveObject;
                }
                else
                {
                    if (ActiveObject.Key == key)
                    {
                        return ActiveObject;
                    }

                    for (int c = 0; c < list.Count; ++c)
                    {
                        if (this[c].Key == key)
                        {
                            return this[c];
                        }
                    }
                }

                return null;
            }
        }

        public virtual bool IsModified
        {
            get
            {
                bool mod = false;

                if (readed)
                {
                    foreach (BusinessBase obj in this)
                    {
                        if (obj.IsModified || obj.IsNew || obj.IsDeleting)
                        {
                            mod = true;
                            break;
                        }
                    }
                }

                return mod;
            }
        }

        public virtual bool IsSomeOneNewOrChanged
        {
            get
            {
                bool mod = false;

                if (readed)
                {
                    foreach (BusinessBase b in this)
                    {
                        if (b.IsNewOrChanged)
                        {
                            mod = true;
                            break;
                        }
                    }
                }

                return mod;
            }
        }

        public int IndexOf(BusinessBase item)
        {
            if (item == null)
            {
                return -1;
            }
            EnsureList();
            return (list.IndexOf(item));
        }

        public void Insert(int index, BusinessBase item)
        {
            EnsureList();
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            EnsureList();
            list.RemoveAt(index);
            Changed();
        }

        public BusinessBase this[int index]
        {
            get
            {
                EnsureList();
                return ((BusinessBase)list[index]);
            }
            set
            {
                EnsureList();
                list[index] = value;
            }
        }

        public void Add(BusinessBase item)
        {
            EnsureList();
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(BusinessBase item)
        {
            EnsureList();
            return list.Contains(item);
        }

        public bool ContainsWithOutEnsureList(BusinessBase item)
        {
            return list.Contains(item);
        }

        public void CopyTo(BusinessBase[] array, int arrayIndex)
        {
            EnsureList();
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                EnsureList();
                return list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(BusinessBase item)
        {
            bool removed = false;
            EnsureList();

            removed = list.Remove(item);

            Changed();

            return removed;
        }

        public IEnumerator<BusinessBase> GetEnumerator()
        {
            EnsureList();
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            EnsureList();
            return list.GetEnumerator();
        }

        public virtual bool AddSimple(int[] ids, string fieldIDName, bool append = false)
        {
            bool chengesMade = false;
            int pos = 0;

            foreach (int id in ids)
            {
                bool found = false;

                foreach (BusinessBase b in this)
                {
                    if ((int)b[fieldIDName] == id)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    this.CreateNew();

                    this.ActiveObject[fieldIDName] = id;

                    if (append)
                    {
                        this.AddActiveObject();
                    }
                    else
                    {
                        this.InsertarActiveObject(pos);
                        pos++;
                    }

                    chengesMade = true;
                }
            }

            return chengesMade;
        }

        public void MoveObject(bool up, string key)
        {
            if (this.Count > 0)
            {
                int pos = this.IndexOf(this[key]);

                if (pos >= 0)
                {
                    int posOther = pos;

                    if (up)
                    {
                        if (pos > 0)
                        {
                            posOther = pos - 1;
                        }
                    }
                    else
                    {
                        if (pos < this.Count - 1)
                        {
                            posOther = pos + 1;
                        }
                    }

                    Swap(pos, posOther);
                }
            }
        }

        public void Swap(int pos1, int pos2)
        {
            if (pos1 != pos2)
            {
                lock (this)
                {
                    BusinessBase tmp = this[pos1];

                    this[pos1] = this[pos2];
                    this[pos2] = tmp;
                }
                //this[pos1].SwapData(this[pos2]); ???

                this[pos1].ClientRefreshPending = true;
                this[pos2].ClientRefreshPending = true;

                ClientRefreshPending = true;
            }
        }

        public string SQLListUsingField(string fieldName)
        {
            string list = "";
            bool isNumeric = ActiveObject.Definition.Properties[fieldName].BasicType
                == BasicType.Number;

            foreach (BusinessBase obj in this)
            {
                if (list != "")
                {
                    list += ",";
                }
                if (isNumeric)
                {
                    list += obj[fieldName].ToString();
                }
                else
                {
                    list += "'" + obj[fieldName].ToString().Replace("'", "''") + "'";
                }
            }

            if (list == "")
            {
                list = (isNumeric ? "0" : "''");
            }

            return list;
        }
    }

    public class BusinessCollectionFiltered : IEnumerable, ICollection
    {
        int count = 0;
        BusinessCollectionFilteredEnumerator filtered;

        public BusinessCollectionFiltered(BusinessCollectionBase col, string filterName)
        {
            filtered = new BusinessCollectionFilteredEnumerator(col, filterName);

            while (filtered.MoveNext())
            {
                count++;
            }
            filtered.Reset();
        }

        #region IEnumerable members
        public IEnumerator GetEnumerator()
        {
            return filtered;
        }
        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        #endregion
    }

    public class BusinessCollectionFilteredEnumerator : IEnumerator
    {
        BusinessBase actual;
        BusinessCollectionBase _col;
        string _filterName;

        int pos;

        public BusinessCollectionFilteredEnumerator(BusinessCollectionBase col, string filterName)
        {
            _col = col;
            _filterName = filterName;
            Reset();
        }

        #region IEnumerator members
        public void Reset()
        {
            actual = null;
            pos = -1;
        }

        public object Current
        {
            get
            {
                return actual;
            }
        }

        public bool MoveNext()
        {
            bool found = false;

            pos++;
            while (pos < _col.Count)
            {
                actual = _col[pos];
                if (actual.MatchFilter(_filterName))
                {
                    found = true;
                    break;
                }
                pos++;
            }

            return found;
        }
        #endregion
    }
}
