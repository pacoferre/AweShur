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

            lazyConnection = new Lazy<IDbConnection>(() => 
            {
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
            HttpContext context = BusinessBaseProvider.HttpContext;

            if (context.Items[key] == null)
            {
                lock (context)
                {
                    if (context.Items[key] == null)
                    {
                        DB d = new DB(dbNumber);

                        context.Items[key] = d;
                    }
                }
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
            if (conn.State != ConnectionState.Closed)
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

            result = conn.Query<T>(sql, param, trans, buffered, commandTimeout ?? secondsTimeOut, commandType);

            CloseConnection();

            return result;
        }

        public IEnumerable<dynamic> Query(string sql, object param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            IEnumerable<dynamic> result;

            OpenConnection();

            result = conn.Query(sql, param, trans, buffered, commandTimeout ?? secondsTimeOut, commandType);

            CloseConnection();

            return result;
        }

        public T QueryFirstOrDefault<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            T result;

            OpenConnection();

            result = conn.QueryFirstOrDefault<T>(sql, param, trans, commandTimeout ?? secondsTimeOut, commandType);

            CloseConnection();

            return result;
        }

        public dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            dynamic result;

            OpenConnection();

            result = conn.QueryFirstOrDefault(sql, param, trans, commandTimeout ?? secondsTimeOut, commandType);

            CloseConnection();

            return result;
        }

        public int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            int result;

            OpenConnection();

            result = conn.Execute(sql, param, trans, commandTimeout, commandType);

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

        public void ReadBusinessCollection(BusinessCollectionBase col)
        {
            OpenConnection();

            conn.ReadBusinessCollection(col, trans);

            CloseConnection();
        }

        public void StoreBusinessObject(BusinessBase obj)
        {
            BusinessBaseDefinition def = obj.Definition;

            OpenConnection();

            if (obj.IsNew)
            {
                if (def.primaryKeyIsOneGuid)
                {
                    obj[def.PrimaryKeys[0]] = GenerateComb();
                }

                var result = conn.QuerySingle(def.InsertQuery, def.GetInsertParameters(obj), trans);

                if (result == null)
                {
                    throw new Exception("Error insering new " + obj.Description);
                }

                if (def.primaryKeyIsOneInt)
                {
                    obj[def.PrimaryKeys[0]] = Convert.ToInt32(result.id);
                }
                if (def.primaryKeyIsOneLong)
                {
                    obj[def.PrimaryKeys[0]] = Convert.ToInt64(result.id);
                }
            }
            else if (obj.IsDeleting)
            {
                int result = conn.Execute(def.DeleteQuery, def.GetPrimaryKeyParameters(obj), trans);

                if (result != 1)
                {
                    throw new Exception("Error deleting " + obj.Description);
                }
            }
            else
            {
                int result = conn.Execute(def.UpdateQuery, def.GetUpdateParameters(obj), trans);

                if (result != 1)
                {
                    throw new Exception("Error updating " + obj.Description);
                }
            }

            CloseConnection();
        }

        // From NHibernate
        private static readonly long BaseDateTicks = new DateTime(1900, 1, 1).Ticks;

        private Guid GenerateComb()
        {
            byte[] guidArray = Guid.NewGuid().ToByteArray();

            DateTime now = DateTime.UtcNow;

            // Get the days and milliseconds which will be used to build the byte string 
            TimeSpan days = new TimeSpan(now.Ticks - BaseDateTicks);
            TimeSpan msecs = now.TimeOfDay;

            // Convert to a byte array 
            // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            // Reverse the bytes to match SQL Servers ordering 
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Copy the bytes into the guid 
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }
    }
}
