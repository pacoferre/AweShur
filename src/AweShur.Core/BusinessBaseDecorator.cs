using AweShur.Core.Lists;
using Dapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBaseDecorator
    {
        public Dictionary<string, PropertyDefinition> Properties { get; } = new Dictionary<string, PropertyDefinition>();
        public List<PropertyDefinition> ListProperties { get; } = new List<PropertyDefinition>();
        public List<int> PrimaryKeys { get; } = new List<int>();
        public bool primaryKeyIsOneInt { get; internal set; }
        public bool primaryKeyIsOneLong { get; internal set; }
        public bool primaryKeyIsOneGuid { get; internal set; }

        public string Singular { get; set; } = "";
        public string Plural { get; set; } = "";
        public string AllListDescription { get; set; } = "All";

        public bool[] setModified;
        private Dictionary<string, int> fieldNameLookup;
        protected DBDialect dialect = null;
        protected int dbNumber;
        protected string tableName;
        protected string tableNameEncapsulated;
        protected string objectName;
        protected string[] names;
        protected PropertyDefinition firstStringProperty;

        private Lazy<string> selectBuilder;
        private Lazy<string> filterSelectBuilder;
        private Lazy<string> insertBuilder;
        private Lazy<string> updateBuilder;
        private Lazy<string> deleteBuilder;

        public BusinessBaseDecorator()
        {
            selectBuilder = new Lazy<string>(PrepareSelectQuery);
            insertBuilder = new Lazy<string>(PrepareInsertQuery);
            updateBuilder = new Lazy<string>(PrepareUpdateQuery);
            deleteBuilder = new Lazy<string>(PrepareDeleteQuery);
        }

        public int DBNumber
        {
            get
            {
                return dbNumber;
            }
        }

        public string TableNameEncapsulated
        {
            get
            {
                return tableNameEncapsulated;
            }
        }

        public string ObjectName
        {
            get
            {
                return objectName;
            }
        }

        internal int IndexOfName(string name)
        {
            int result;

            return (name != null &&
                fieldNameLookup.TryGetValue(name, out result)) ? result : -1;
        }

        public virtual void SetProperties(string tableName, int dbNumber)
        {
            this.dbNumber = dbNumber;
            objectName = tableName;
            dialect = DB.InstanceNumber(dbNumber).Dialect;
            this.tableName = tableName;
            tableNameEncapsulated = dialect.Encapsulate(dialect.GetFullTableName(tableName));

            foreach (DBDialect.ColumnDefinition column in dialect.GetSchema(
                tableNameEncapsulated, dbNumber))
            {
                PropertyDefinition def = new PropertyDefinition(column);

                Properties[column.ColumnName] = def;
            }

            if (Properties.Count == 0)
            {
                throw new Exception("No Schema found for " + tableName + ".");
            }

            Singular = "";
            foreach (char letter in objectName)
            {
                if (Singular != "" && letter.ToString().ToUpper() == letter.ToString())
                {
                    Singular += " ";
                }

                Singular += letter;
            }

            Plural = Singular + "s";

            SetCustomProperties();

            PostSetCustomProperties();
        }

        protected virtual void SetCustomProperties()
        {
        }

        protected void PostSetCustomProperties()
        { 
            ListProperties.AddRange(Properties.Values.ToList());
            fieldNameLookup = new Dictionary<string, int>(Properties.Count, StringComparer.Ordinal);

            setModified = new bool[ListProperties.Count];

            if (firstStringProperty == null)
            {
                firstStringProperty = ListProperties.Find(prop => prop.BasicType == BasicType.Text && !prop.IsIdentity);
            }

            names = Properties.Keys.ToArray();
            for (int i = 0; i < Properties.Count; i++)
            {
                PropertyDefinition prop = Properties.ElementAt(i).Value;

                fieldNameLookup[names[i]] = i;
                setModified[i] = prop.SetModified;

                if (prop.IsPrimaryKey)
                {
                    PrimaryKeys.Add(i);
                }

                prop.Index = i;

                if (prop.Type == PropertyInputType.select)
                {
                    prop.DefaultSearch = "0";
                }
            }

            primaryKeyIsOneInt = PrimaryKeys.Count == 1 && 
                ListProperties[PrimaryKeys[0]].DataType == typeof(Int32);
            primaryKeyIsOneLong = PrimaryKeys.Count == 1 && 
                ListProperties[PrimaryKeys[0]].DataType == typeof(Int64);
            primaryKeyIsOneGuid = PrimaryKeys.Count == 1 && 
                ListProperties[PrimaryKeys[0]].DataType == typeof(Guid);
        }

        public virtual DataItem New(BusinessBase owner)
        {
            object[] values = new object[ListProperties.Count];

            foreach(PropertyDefinition prop in ListProperties)
            { 
                values[prop.Index] = prop.DefaultValue;
            }

            return new DataItem(owner, values);
        }

        public PropertyDefinition FirstStringProperty
        {
            get
            {
                return firstStringProperty;
            }
        }

        protected virtual string PrepareSelectQuery()
        {
            StringBuilder sql = new StringBuilder("select ");

            sql.Append(dialect.SQLAllColumns(ListProperties));
            sql.Append(" from " + tableNameEncapsulated);
            sql.Append(" where " + dialect.SQLWherePrimaryKey(ListProperties));

            return sql.ToString();
        }

        public string SelectQuery
        {
            get
            {
                return selectBuilder.Value;
            }
        }

        protected virtual string PrepareInsertQuery()
        {
            StringBuilder sql = new StringBuilder("insert into " + tableNameEncapsulated);

            sql.Append(" (" + dialect.SQLInsertProperties(ListProperties) + ")");
            sql.Append(" values (" + dialect.SQLInsertValues(ListProperties) + ") ");

            if (primaryKeyIsOneInt || primaryKeyIsOneLong)
            {
                if (dialect.Dialect == DBDialectEnum.PostgreSQL) //???
                {
                    sql.Append(" RETURNING lastval() as id");
                }
                else
                {
                    sql.Append(";" + dialect.GetIdentitySql);
                }
            }

            return sql.ToString();
        }

        public string InsertQuery
        {
            get
            {
                return insertBuilder.Value;
            }
        }

        protected virtual string PrepareUpdateQuery()
        {
            StringBuilder sql = new StringBuilder("update " + tableNameEncapsulated);

            sql.Append(" set " + dialect.SQLUpdatePropertiesValues(ListProperties));
            sql.Append(" where " + dialect.SQLWherePrimaryKey(ListProperties));

            return sql.ToString();
        }

        public string UpdateQuery
        {
            get
            {
                return updateBuilder.Value;
            }
        }

        protected virtual string PrepareDeleteQuery()
        {
            StringBuilder sql = new StringBuilder("delete");

            sql.Append(" from " + tableNameEncapsulated);
            sql.Append(" where " + dialect.SQLWherePrimaryKey(ListProperties));

            return sql.ToString();
        }

        public string DeleteQuery
        {
            get
            {
                return deleteBuilder.Value;
            }
        }

        public DynamicParameters GetPrimaryKeyParameters(BusinessBase obj)
        {
            DynamicParameters dynParms = new DynamicParameters();

            foreach (int pos in PrimaryKeys)
            {
                dynParms.Add("@" + names[pos], obj[pos]);
            }

            return dynParms;
        }

        public DynamicParameters GetInsertParameters(BusinessBase obj)
        {
            DynamicParameters dynParms = new DynamicParameters();

            for (int index = 0; index < names.Length; ++index)
            {
                PropertyDefinition prop = ListProperties[index];
                if (prop.IsDBField && !prop.IsIdentity)
                {
                    dynParms.Add("@" + names[index], obj[index]);
                }
            }

            return dynParms;
        }

        public DynamicParameters GetUpdateParameters(BusinessBase obj)
        {
            DynamicParameters dynParms = new DynamicParameters();

            for (int index = 0; index < names.Length; ++index)
            {
                PropertyDefinition prop = ListProperties[index];

                if (prop.IsDBField && !prop.IsReadOnly && !prop.IsComputed)
                {
                    dynParms.Add("@" + names[index], obj[index]);
                }
            }

            return dynParms;
        }

        public virtual string GetListSQL(string listName, string parameter)
        {
            return "Select " + dialect.Encapsulate(names[PrimaryKeys[0]]) + " As ID, "
                + dialect.Encapsulate(firstStringProperty.FieldName) + " From " + tableNameEncapsulated
                + " Order By " + dialect.Encapsulate(firstStringProperty.FieldName);
        }

        public virtual Dictionary<string, object> GetParameter(string parameter)
        {
            if (parameter == "" || parameter == "0")
            {
                return null;
            }

            return new Dictionary<string, object>() { { "id", parameter } };
        }

        public virtual ListTable GetList(string listName = "", string parameter = "")
        {
            string sql = GetListSQL(listName, parameter);
            ListTable list = new ListTable(listName, sql, GetParameter(parameter), this.DBNumber, AllListDescription);

            return list;
        }
    }
}
