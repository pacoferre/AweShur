using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using AweShur.Core.Security;
using Microsoft.Extensions.Configuration;
using System.Text;
using StackExchange.Redis;
using AweShur.Core.Lists;

namespace AweShur.Core
{
    public class BusinessBaseProvider
    {
        protected Dictionary<string, Func<BusinessBase>> creators = new Dictionary<string, Func<BusinessBase>>();
        protected Dictionary<string, Func<BusinessBaseDefinition>> decorators = new Dictionary<string, Func<BusinessBaseDefinition>>();
        private ConcurrentDictionary<string, Lazy<BusinessBaseDefinition>>
            definitionsCreators = new ConcurrentDictionary<string, Lazy<BusinessBaseDefinition>>();
        public static BusinessBaseProvider Instance { get; set; }
        private static IHttpContextAccessor HttpContextAccessor;
        private static ConnectionMultiplexer TheCache;
        public static ListProvider ListProvider { get; private set; }

        public static void Configure(IHttpContextAccessor httpContextAccessor, ConnectionMultiplexer theCache,
            BusinessBaseProvider instance, IConfigurationRoot configuration)
        {
            HttpContextAccessor = httpContextAccessor;
            TheCache = theCache;
            Instance = instance;
            DB.Configuration = configuration;

            Instance.RegisterBusinessCreators();

            AppUser.SALT = Encoding.ASCII.GetBytes(DB.Configuration.GetSection("Security")["SALT"]).Take(16).ToArray();

            ListProvider = new ListProvider();
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
            creators.Add("AppUser", () => new Security.AppUser());
            decorators.Add("AppUser", () => new Security.AppUserDecorator());

            creators.Add("AppUserNoDB", () => new Security.AppUserNoDB());
            decorators.Add("AppUserNoDB", () => new Security.AppUserNoDBDecorator());
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

        public BusinessBaseDefinition GetDefinition(BusinessBase business)
        {
            return GetDefinition(business.TableName, business.DBNumber);
        }

        public BusinessBaseDefinition GetDefinition(string name, int dbNumber)
        {
            Lazy<BusinessBaseDefinition> lazy = definitionsCreators.GetOrAdd(
                name,
                new Lazy<BusinessBaseDefinition>(
                    () => GetDefinitionInternal(name, dbNumber),
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

        public FilterBase GetFilter(AppUser user, string objectName)
        {
            return new FilterBase(GetDefinition(objectName, 0), 0);
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

        public static bool ExistsData(string key)
        {
            return TheCache.GetDatabase().KeyExists(key);
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
            object objTemp;
            BusinessBase obj;
            byte[] data;

            if (context.Items.TryGetValue(objectKey, out objTemp))
            {
                obj = (BusinessBase)objTemp;
            }
            else
            {
                bool readFromDB = true;

                obj = Instance.CreateObject(objectName, dbNumber);

                data = GetData(objectKey);

                if (data != null)
                {
                    try
                    {
                        obj.Deserialize(data);
                        readFromDB = false;
                    }
                    catch
                    {
                        // Sometimes Redis returns bad data.
                    }
                }
                if (readFromDB)
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

            return obj;
        }
    }
}
