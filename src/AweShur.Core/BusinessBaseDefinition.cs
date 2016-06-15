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
        private Dictionary<string, int> fieldNameLookup;
        protected DBDialect dialect;
        protected string tableName;
        protected Dictionary<string, PropertyDefinition> properties;
        protected string[] names;
        protected List<int> primaryKeys;

        private Lazy<string> selectBuilder;
        private Lazy<string> insertBuilder;
        private Lazy<string> updateBuilder;
        private Lazy<string> deleteBuilder;

        public BusinessBaseDefinition()
        {

            selectBuilder = new Lazy<string>(PrepareSelectQuery);
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

                properties[column.ColumnName] = def;
            }

            SetCustomProperties();

            fieldNameLookup = new Dictionary<string, int>(properties.Count, StringComparer.Ordinal);

            names = properties.Keys.ToArray();
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDefinition prop = properties.ElementAt(i).Value;

                fieldNameLookup[names[i]] = i;

                if (prop.IsPrimaryKey)
                {
                    primaryKeys.Add(i);
                }
            }
        }

        protected virtual void SetCustomProperties()
        {
        }

        public virtual DataItem New(BusinessBase owner)
        {
            object[] values = new object[properties.Count];

            for (int i = 0; i < properties.Count; i++)
            {
                values[i] = properties.ElementAt(i).Value.DefaultValue;
            }

            return new DataItem(owner, values);
        }

        protected virtual string PrepareSelectQuery()
        {
            StringBuilder sql = new StringBuilder("select ");

            sql.Append(dialect.SQLAllColumns(properties.Values.ToList()));
            sql.Append(" from " + dialect.Encapsulate(tableName));
            sql.Append(" where " + dialect.SQLWherePrimaryKey(properties.Values.ToList()));

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
            string sql = "";

            return sql;
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
            string sql = "";

            return sql;
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

            foreach (int pos in primaryKeys)
            {
                dynParms.Add("@" + names[pos], obj[pos]);
            }

            return dynParms;
        }
    }
}
