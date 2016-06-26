using System;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AweShur.Core.Security;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace AweShur.Core
{
    public class BusinessBaseProvider
    {
        private Dictionary<string, Func<BusinessBase>> creators = new Dictionary<string, Func<BusinessBase>>();
        private Dictionary<string, Func<BusinessBaseDefinition>> decorators = new Dictionary<string, Func<BusinessBaseDefinition>>();
        private ConcurrentDictionary<string, Lazy<BusinessBaseDefinition>>
            definitionsCache = new ConcurrentDictionary<string, Lazy<BusinessBaseDefinition>>();
        public static BusinessBaseProvider Instance { get; set; }
        private static IHttpContextAccessor HttpContextAccessor;
        private static ConnectionMultiplexer TheCache;

        public static void Configure(IHttpContextAccessor httpContextAccessor, ConnectionMultiplexer theCache,
            BusinessBaseProvider instance, IConfigurationRoot configuration)
        {
            HttpContextAccessor = httpContextAccessor;
            TheCache = theCache;
            Instance = instance;
            DB.Configuration = configuration;

            Instance.RegisterBusinessCreators();
            Instance.RegisterCustomDecorators();

            AppUser.SALT = Encoding.ASCII.GetBytes(DB.Configuration.GetSection("Security")["SALT"]).Take(16).ToArray();
        }

        public static HttpContext HttpContext
        {
            get
            {
                return HttpContextAccessor.HttpContext;
            }
        }

        public virtual void RegisterBusinessCreators()
        {
            creators.Add("AppUser", () => new Security.AppUser() );
            decorators.Add("AppUser", () => new Security.AppUserDecorator() );
        }

        public virtual void RegisterCustomDecorators()
        {

        }

        public virtual BusinessBase CreateObject(string objectName, int dbNumber = 0)
        {
            BusinessBase obj;
            Func<BusinessBase> creator;

            if (creators.TryGetValue(objectName, out creator))
            {
                obj = creator.Invoke();

                if (dbNumber != 0)
                {
                    //obj.ChangeDBNumber(dbNumber);
                }
            }
            else
            {
                obj = new BusinessBase(objectName, dbNumber);
            }

            return obj;
        }

        public virtual BusinessBaseDefinition GetDefinition(BusinessBase business)
        {
            Lazy<BusinessBaseDefinition> lazy = definitionsCache.GetOrAdd(
                business.TableName,
                new Lazy<BusinessBaseDefinition>(
                    () => GetDefinitionInternal(business.TableName, business.DBNumber),
                    LazyThreadSafetyMode.ExecutionAndPublication
                ));

            return lazy.Value;
        }

        private BusinessBaseDefinition GetDefinitionInternal(string tableName, int dbNumber)
        {
            BusinessBaseDefinition definition;

            if (decorators.ContainsKey(tableName))
            {
                definition = decorators[tableName].Invoke();
            }
            else
            {
                definition = new BusinessBaseDefinition();
            }
            definition.SetProperties(tableName, dbNumber);

            return definition;
        }

        private static string ObjectKey(string objectName, int dbNumber, string key)
        {
            return "O_" + objectName + "_" + dbNumber + "_" + key;
        }

        public static void StoreObject(BusinessBase obj, string objectName)
        {
            string objectKey = ObjectKey(objectName, obj.DBNumber, obj.Key);

            StoreData(objectKey, obj.Serialize());
        }

        public static void StoreData(string key, byte[] data)
        {
            TheCache.GetDatabase().StringSetAsync(key, data, TimeSpan.FromMinutes(30), When.Always, CommandFlags.FireAndForget);
        }

        public static byte[] GetData(string key)
        {
            return TheCache.GetDatabase().StringGet(key);
        }

        public static void RemoveData(string key)
        {
            TheCache.GetDatabase().KeyDeleteAsync(key, CommandFlags.FireAndForget);
        }

        public static BusinessBase RetreiveObject(HttpContext context, string objectName, string key)
        {
            return RetreiveObject(context, objectName, 0, key);
        }

        public static BusinessBase RetreiveObject(HttpContext context, string objectName, int dbNumber, string key)
        {
            string objectKey = ObjectKey(objectName, dbNumber, key);
            byte[] data;

            if (context.Items[objectKey] == null)
            {
                lock (context)
                {
                    if (context.Items[objectKey] == null)
                    {
                        BusinessBase obj = Instance.CreateObject(objectName, dbNumber);

                        data = GetData(objectKey);

                        if (data != null)
                        {
                            obj.Deserialize(data);
                        }
                        else
                        {
                            if (key != "0")
                            {
                                obj.ReadFromDB(key);
                            }
                            else
                            {
                                if (!obj.IsNew)
                                {
                                    obj.SetNew();
                                }
                            }

                            StoreObject(obj, objectName);
                        }

                        context.Items[objectKey] = obj;
                    }
                }
            }

            return (BusinessBase)context.Items[objectKey];
        }
    }
}
