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
    public class DB : IDisposable
    {
        public static IConfigurationRoot Configuration { get; set; }

        public DBDialect Dialect { get; }
        public int DBNumber { get; }
        private string connectionString;
        private Lazy<IDbConnection> lazyConnection;
        private IDbConnection conn = null;
        private IDbTransaction trans = null;
        private bool inTransaction = false;
        private int secondsTimeOut = 240;

        public DB(int dbNumber)
        {
            IConfigurationSection section = Configuration.GetSection("Data").GetSection("Instance_" + dbNumber);

            DBNumber = dbNumber;

            Dialect = DBDialect.Instance((DBDialectEnum) Enum.Parse(typeof(DBDialectEnum), section["Dialect"]));
            connectionString = section["ConnectionString"];

            lazyConnection = new Lazy<IDbConnection>(() => {
                return Dialect.GetConnection(connectionString);
            }, false);
        }

        public void Dispose()
        {
            try
            {
                CloseConnection(true);
                conn.Dispose();
                conn = null;
            }
            catch
            {
            }
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

        private void OpenConnection()
        {
            if (conn == null)
            {
                conn = lazyConnection.Value;
            }

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        private void CloseConnection()
        {
            if (!inTransaction)
            {
                CloseConnection(false);
            }
        }

        private void CloseConnection(bool forceClose)
        {
            lock (this)
            {
                if (conn.State != ConnectionState.Closed)
                {
                    if (inTransaction && forceClose)
                    {
                        RollBackTransaction();

                        throw new Exception("Can't close with open transaction. RollBack done.");
                    }
                    else
                    {
                        if (!inTransaction)
                        {
                            conn.Close();
                        }
                    }
                }
            }
        }

        public void BeginTransaction()
        {
            lock (this)
            {
                if (inTransaction)
                {
                    throw new System.Exception("Can't start transaction twice.");
                }

                OpenConnection();
                trans = conn.BeginTransaction();
                inTransaction = true;
            }
        }

        public bool InTransaction
        {
            get
            {
                return inTransaction;
            }
        }

        public void RollBackTransaction()
        {
            lock (this)
            {
                if (!inTransaction)
                {
                    throw new System.Exception("Can't Rollback without BeginTransaction.");
                }

                trans.Rollback();
                inTransaction = false;
                trans = null;
                CloseConnection();
            }
        }

        public void CommitTransaction()
        {
            if (!inTransaction)
            {
                throw new System.Exception("Can't Commit without BeginTransaction.");
            }

            trans.Commit();
            inTransaction = false;
            trans = null;
            CloseConnection();
        }

        public IEnumerable<T> Query<T>(string sql, object param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            IEnumerable<T> result;

            OpenConnection();

            result = conn.Query<T>(sql, param, trans, buffered, commandTimeout, commandType);

            CloseConnection();

            return result;
        }

        public void ReadBusinessObject(BusinessBase obj)
        {
            BusinessBaseDefinition def = obj.Definition;

            OpenConnection();

            conn.ReadBusinessObject(obj, def.SelectQuery, def.GetPrimaryKeyParameters(obj), trans);

            CloseConnection();
        }
    }
}
