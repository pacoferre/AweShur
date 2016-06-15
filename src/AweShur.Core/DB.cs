using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AweShur.Core
{
    public class DB
    {
        public static IConfigurationRoot Configuration { get; set; }

        public DBDialect Dialect { get; }
        public int DBNumber { get; }
        private string connectionString;

        public DB(int dbNumber)
        {
            //      "Data": {
            //          "Instance_0": {
            //              "Dialect": "SQLServer",
            //              "ConnectionString": "..."
            //          }
            //      }

            IConfigurationSection section = Configuration.GetSection("AppSettings").GetSection("Instance_" + dbNumber);

            DBNumber = dbNumber;

            Dialect = DBDialect.Instance((DBDialectEnum) Enum.Parse(typeof(DBDialectEnum), section["Dialect"]));
            connectionString = section["ConnectionString"];
        }

        public static DB Instance
        {
            get
            {
                return InstanceNumber(0);
            }
        }

        public static DB InstanceNumber(int dbNumber)
        {
            string key = "DB_" + dbNumber.ToString();
            HttpContext context = (new HttpContextAccessor()).HttpContext;

            if (context.Items[key] == null)
            {
                DB d = new DB(dbNumber);

                context.Items[key] = d;
            }

            return (DB)context.Items[key];
        }

        private IDbConnection Open()
        {
            return Dialect.GetConnection(connectionString);
        }

        public IEnumerable<T> Query<T>(string sql, object param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (IDbConnection conn = Open())
            {
                return conn.Query<T>(sql, param, null, buffered, commandTimeout, commandType);
            }
        }

        public void ReadBusinessObject(BusinessBase obj)
        {
            using (IDbConnection conn = Open())
            {
                BusinessBaseDefinition def = obj.Definition;

                conn.ReadBusinessObject(obj, def.SelectQuery, def.GetPrimaryKeyParameters(obj));

                conn.Close();
            }
        }
    }
}
