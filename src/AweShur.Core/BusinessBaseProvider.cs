using System;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Framework.Runtime.Infrastructure;
using Microsoft.AspNetCore.Http;
using AweShur.Core.Security;
using Microsoft.Extensions.Configuration;
using System.Text;

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

        public static void Configure(IHttpContextAccessor httpContextAccessor, BusinessBaseProvider instance, IConfigurationRoot configuration)
        {
            HttpContextAccessor = httpContextAccessor;
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

        private static string SessionKey(string objectName, int dbNumber, string key)
        {
            return "O_" + objectName + "_" + dbNumber + "_" + key;
        }

        public static void StoreObject(BusinessBase obj, string objectName, HttpContext context)
        {
            string sessionKey = SessionKey(objectName, obj.DBNumber, obj.Key);

            context.Session.Set(sessionKey, obj.Serialize());
        }

        public static BusinessBase RetreiveObject(string objectName, string key, HttpContext context)
        {
            return RetreiveObject(objectName, 0, key, context);
        }

        public static BusinessBase RetreiveObject(string objectName, int dbNumber, string key, HttpContext context)
        {
            string sessionKey = SessionKey(objectName, dbNumber, key);
            byte[] data;

            if (context.Items[sessionKey] == null)
            {
                lock (context)
                {
                    if (context.Items[sessionKey] == null)
                    {
                        BusinessBase obj = Instance.CreateObject(objectName, dbNumber);

                        if (context.Session.TryGetValue(sessionKey, out data))
                        {
                            obj.Deserialize(data);
                        }
                        else
                        {
                            if (key != "0")
                            {
                                obj.ReadFromDB(key);
                            }

                            StoreObject(obj, objectName, context);
                        }

                        context.Items[sessionKey] = obj;
                    }
                }
            }

            return (BusinessBase)context.Items[sessionKey];
        }
    }
}
