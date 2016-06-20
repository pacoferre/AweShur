using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public enum DBDialectEnum
    {
        SQLServer,
        PostgreSQL,
        SQLite,
        MySQL,
    }

    public class DBDialect
    {
        private static readonly Lazy<DBDialect>[] _instance
            = { new Lazy<DBDialect>(() => new DBDialect(DBDialectEnum.SQLServer)),
                new Lazy<DBDialect>(() => new DBDialect(DBDialectEnum.PostgreSQL)),
                new Lazy<DBDialect>(() => new DBDialect(DBDialectEnum.SQLite)),
                new Lazy<DBDialect>(() => new DBDialect(DBDialectEnum.MySQL)) };

        public DBDialectEnum Dialect { get; }
        public string Encapsulation { get; }
        public string GetIdentitySql { get; }
        public string GetPagedListSql { get; }
        public string GetSchemaSql { get; }

        private static DBDialect Retreive(DBDialectEnum dialect)
        {
            return new DBDialect(dialect);
        }

        public class ColumnDefinition
        {
            public string ColumnName;
            public string DBDataType;
            public int MaxLength;
            public bool IsNullable;
            public bool IsIdentity;
            public bool IsPrimaryKey;
            public bool IsComputed;
        }

        private static Dictionary<string, Type> typesDict = new Dictionary<string, Type>() {
            { "int", typeof(Int32) },
            { "bit", typeof(bool) },
            { "bigint", typeof(Int64) },
            { "smallint", typeof(Int16) },
            { "tinyint", typeof(Byte) },
            { "real", typeof(Single) },
            { "float", typeof(Double) },
            { "nvarchar", typeof(string) },
            { "varchar", typeof(string) },
            { "date", typeof(DateTime) },
            { "datetime", typeof(DateTime) },
            { "ntext", typeof(string) },
            { "text", typeof(string) },
            { "nchar", typeof(string) },
            { "char", typeof(string) },
            { "money", typeof(Decimal) },
            { "decimal", typeof(Decimal) },
            { "timestamp", typeof(Byte[]) },
            { "binary", typeof(Byte[]) },
            { "uniqueidentifier", typeof(Guid) },
        };

        private static Dictionary<Type, BasicType> basicTypesDict = new Dictionary<Type, BasicType>() {
            { typeof(Int32), BasicType.Number },
            { typeof(bool), BasicType.Bit },
            { typeof(Int64), BasicType.Number },
            { typeof(Int16), BasicType.Number },
            { typeof(Byte), BasicType.Number },
            { typeof(Single), BasicType.Number },
            { typeof(Double), BasicType.Number },
            { typeof(string), BasicType.Text },
            { typeof(DateTime), BasicType.DateTime },
            { typeof(Decimal), BasicType.Number },
            { typeof(Byte[]), BasicType.Bynary },
            { typeof(Guid), BasicType.GUID },
        };

        public static Type GetColumnType(string dbDataType)
        {
            return typesDict[dbDataType];
        }

        public static BasicType GetBasicType(Type type)
        {
            return basicTypesDict[type];
        }

        public IEnumerable<ColumnDefinition> GetSchema(string tableName, int dbNumber = 0)
        {
            string tableSchema = "dbo";
            
            if (tableName.Contains('.'))
            {
                string[] parts = tableName.Split('.');

                tableSchema = parts[0];
                tableName = parts[1];
            }

            return DB.InstanceNumber(dbNumber).Query<ColumnDefinition>(GetSchemaSql, new { TableName = tableName, TableSchema = tableSchema });
        }

        public IDbConnection GetConnection(string connectionString)
        {
            IDbConnection conn = null;

            switch (Dialect)
            {
                case DBDialectEnum.PostgreSQL:
                    conn = new Npgsql.NpgsqlConnection(connectionString);
                    break;
                case DBDialectEnum.SQLite:
                    throw new Exception("To come for .NET Core 1.0");
//                    break;
                case DBDialectEnum.MySQL:
                    throw new Exception("To come for .NET Core 1.0");
//                    break;
                default:
                    conn = new System.Data.SqlClient.SqlConnection(connectionString);
                    break;
            }

            return conn;
        }

        private DBDialect(DBDialectEnum dialect)
        {
            switch (dialect)
            {
                case DBDialectEnum.PostgreSQL:
                    Dialect = DBDialectEnum.PostgreSQL;
                    Encapsulation = "{0}";
                    GetIdentitySql = string.Format("SELECT LASTVAL() AS id");
                    GetPagedListSql = "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
                    break;
                case DBDialectEnum.SQLite:
                    Dialect = DBDialectEnum.SQLite;
                    Encapsulation = "{0}";
                    GetIdentitySql = string.Format("SELECT LAST_INSERT_ROWID() AS id");
                    GetPagedListSql = "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
                    break;
                case DBDialectEnum.MySQL:
                    Dialect = DBDialectEnum.MySQL;
                    Encapsulation = "`{0}`";
                    GetIdentitySql = string.Format("SELECT LAST_INSERT_ID() AS id");
                    GetPagedListSql = "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {Offset},{RowsPerPage}";
                    break;
                default:
                    Dialect = DBDialectEnum.SQLServer;
                    Encapsulation = "[{0}]";
                    GetIdentitySql = string.Format("SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS [id]");
                    GetPagedListSql = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {OrderBy}) AS PagedNumber, {SelectColumns} FROM {TableName} {WhereClause}) AS u WHERE PagedNUMBER BETWEEN (({PageNumber}-1) * {RowsPerPage} + 1) AND ({PageNumber} * {RowsPerPage})";
                    GetSchemaSql = @"SELECT col.COLUMN_NAME AS ColumnName
, col.DATA_TYPE AS DBDataType
, col.CHARACTER_MAXIMUM_LENGTH AS MaxLength
, CAST(CASE col.IS_NULLABLE
WHEN 'NO' THEN 0
ELSE 1
END AS bit) AS IsNullable
, COLUMNPROPERTY(OBJECT_ID('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']'), col.COLUMN_NAME, 'IsIdentity') AS IsIdentity
, CAST(ISNULL(pk.is_primary_key, 0) AS bit) AS IsPrimaryKey
, CAST((case when col.COLUMN_DEFAULT is NULL OR 
COLUMNPROPERTY(OBJECT_ID('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']'), col.COLUMN_NAME, 'IsComputed') = 1 then 0 else 1 end) As bit) AS IsComputed
FROM INFORMATION_SCHEMA.COLUMNS AS col
LEFT JOIN(SELECT SCHEMA_NAME(o.schema_id)AS TABLE_SCHEMA
, o.name AS TABLE_NAME
, c.name AS COLUMN_NAME
, i.is_primary_key
FROM sys.indexes AS i JOIN sys.index_columns AS ic ON i.object_id = ic.object_id
AND i.index_id = ic.index_id
JOIN sys.objects AS o ON i.object_id = o.object_id
LEFT JOIN sys.columns AS c ON ic.object_id = c.object_id
AND c.column_id = ic.column_id
WHERE i.is_primary_key = 1) AS pk ON col.TABLE_NAME = pk.TABLE_NAME
AND col.TABLE_SCHEMA = pk.TABLE_SCHEMA
AND col.COLUMN_NAME = pk.COLUMN_NAME
WHERE col.TABLE_NAME = @TableName
AND col.TABLE_SCHEMA = @TableSchema
ORDER BY col.ORDINAL_POSITION";
                    break;
            }
        }

        public static DBDialect Instance(DBDialectEnum dialect)
        {
            return _instance[(int)dialect].Value;
        }

        public string SQLAllColumns(List<PropertyDefinition> properties)
        {
            StringBuilder sb = new StringBuilder();
            var addedAny = false;

            foreach (PropertyDefinition prop in properties)
            {
                if (prop.IsDBField)
                {
                    if (addedAny)
                    {
                        sb.Append(",");
                    }
                    sb.Append(Encapsulate(prop.PropertyName));

                    addedAny = true;
                }
            }

            return sb.ToString();
        }

        public string SQLWherePrimaryKey(List<PropertyDefinition> properties)
        {
            StringBuilder sb = new StringBuilder();
            var addedAny = false;

            foreach (PropertyDefinition prop in properties)
            {
                if (prop.IsDBField && prop.IsPrimaryKey)
                {
                    if (addedAny)
                    {
                        sb.Append(" AND ");
                    }
                    sb.Append(Encapsulate(prop.PropertyName) + "=@" + prop.PropertyName);

                    addedAny = true;
                }
            }

            return sb.ToString();
        }

        public string SQLInsertProperties(List<PropertyDefinition> properties)
        {
            StringBuilder sb = new StringBuilder();
            var addedAny = false;

            foreach (PropertyDefinition prop in properties)
            {
                if (prop.IsDBField && !prop.IsIdentity)
                {
                    if (addedAny)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(Encapsulate(prop.PropertyName));

                    addedAny = true;
                }
            }

            return sb.ToString();
        }

        public string SQLInsertValues(List<PropertyDefinition> properties)
        {
            StringBuilder sb = new StringBuilder();
            var addedAny = false;

            foreach (PropertyDefinition prop in properties)
            {
                if (prop.IsDBField && !prop.IsIdentity)
                {
                    if (addedAny)
                    {
                        sb.Append(", ");
                    }
                    sb.Append("@" + prop.PropertyName);

                    addedAny = true;
                }
            }

            return sb.ToString();
        }

        public string SQLUpdateValues(List<PropertyDefinition> properties)
        {
            StringBuilder sb = new StringBuilder();
            var addedAny = false;

            foreach (PropertyDefinition prop in properties)
            {
                if (prop.IsDBField && !prop.IsIdentity && !prop.IsReadOnly && !prop.IsComputed)
                {
                    if (addedAny)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(Encapsulate(prop.PropertyName) + "=@" + prop.PropertyName);

                    addedAny = true;
                }
            }

            return sb.ToString();
        }

        public string Encapsulate(string databaseword)
        {
            return string.Format(Encapsulation, databaseword);
        }
    }
}
