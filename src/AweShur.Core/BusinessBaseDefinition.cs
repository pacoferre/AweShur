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
    public class BusinessBaseDefinition
    {
        public Dictionary<string, PropertyDefinition> Properties { get; } = new Dictionary<string, PropertyDefinition>();
        public List<PropertyDefinition> ListProperties { get; } = new List<PropertyDefinition>();
        public List<int> PrimaryKeys { get; } = new List<int>();
        public bool primaryKeyIsOneInt { get; internal set; }
        public bool primaryKeyIsOneLong { get; internal set; }
        public bool primaryKeyIsOneGuid { get; internal set; }
        private bool primaryKeyIsReturned = false;

        public string Singular { get; set; } = "";
        public string Plural { get; set; } = "";

        private Dictionary<string, int> fieldNameLookup;
        protected DBDialect dialect;
        protected string tableName;
        protected string[] names;
        protected PropertyDefinition firstStringProperty;

        private Lazy<string> selectBuilder;
        private Lazy<string> filterSelectBuilder;
        private Lazy<string> insertBuilder;
        private Lazy<string> updateBuilder;
        private Lazy<string> deleteBuilder;

        public BusinessBaseDefinition()
        {
            selectBuilder = new Lazy<string>(PrepareSelectQuery);
            filterSelectBuilder = new Lazy<string>(PrepareFilterSelectQuery);
            insertBuilder = new Lazy<string>(PrepareInsertQuery);
            updateBuilder = new Lazy<string>(PrepareUpdateQuery);
            deleteBuilder = new Lazy<string>(PrepareDeleteQuery);
        }

        internal int IndexOfName(string name)
        {
            int result;

            return (name != null &&
                fieldNameLookup.TryGetValue(name, out result)) ? result : -1;
        }

        public void SetProperties(string tableName, int dbNumber)
        {
            this.tableName = tableName;
            dialect = DB.InstanceNumber(dbNumber).Dialect;

            foreach (DBDialect.ColumnDefinition column in dialect.GetSchema(tableName, dbNumber))
            {
                PropertyDefinition def = new PropertyDefinition(column);

                Properties[column.ColumnName] = def;
            }

            Singular = tableName;
            Plural = tableName + "s";

            SetCustomProperties();

            ListProperties.AddRange(Properties.Values.ToList());
            fieldNameLookup = new Dictionary<string, int>(Properties.Count, StringComparer.Ordinal);

            firstStringProperty = ListProperties.Find(prop => prop.BasicType == BasicType.Text);

            names = Properties.Keys.ToArray();
            for (int i = 0; i < Properties.Count; i++)
            {
                PropertyDefinition prop = Properties.ElementAt(i).Value;

                fieldNameLookup[names[i]] = i;

                if (prop.IsPrimaryKey)
                {
                    PrimaryKeys.Add(i);
                }

                prop.Index = i;
            }

            primaryKeyIsOneInt = PrimaryKeys.Count == 1 && ListProperties[PrimaryKeys[0]].DataType == typeof(Int32);
            primaryKeyIsOneLong = PrimaryKeys.Count == 1 && ListProperties[PrimaryKeys[0]].DataType == typeof(Int64);
            primaryKeyIsOneGuid = PrimaryKeys.Count == 1 && ListProperties[PrimaryKeys[0]].DataType == typeof(Guid);

        }

        protected virtual void SetCustomProperties()
        {
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
            sql.Append(" from " + dialect.Encapsulate(tableName));
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

        protected virtual string PrepareFilterSelectQuery()
        {
            StringBuilder sql = new StringBuilder("select ");

            sql.Append(dialect.SQLListColumns(ListProperties));
            sql.Append(" from " + dialect.Encapsulate(tableName));

            return sql.ToString();
        }

        public string FilterSelectQuery
        {
            get
            {
                return filterSelectBuilder.Value;
            }
        }

        protected virtual string PrepareInsertQuery()
        {
            StringBuilder sql = new StringBuilder("insert into " + dialect.Encapsulate(tableName));

            sql.Append(" (" + dialect.SQLInsertProperties(ListProperties) + ")");
            sql.Append(" values (" + dialect.SQLInsertValues(ListProperties) + ") ");

            primaryKeyIsReturned = false;
            if (primaryKeyIsOneInt || primaryKeyIsOneLong)
            {
                primaryKeyIsReturned = true;

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
            StringBuilder sql = new StringBuilder("update " + dialect.Encapsulate(tableName));

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
            string sql = "";

            return sql;
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
    }
}
