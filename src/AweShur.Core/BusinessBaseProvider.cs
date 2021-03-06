﻿using System;
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
        public static Dictionary<string, string> TableSchemas { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, string> ObjectToDBTable { get; set; } = new Dictionary<string, string>();

        public Func<string, int, BusinessBase> DefaultBusinessBase =
            (objectName, dbNumber) => new BusinessBase(objectName, dbNumber);
        public Func<BusinessBaseDecorator> DefaultBusinessBaseDecorator =
            () => new BusinessBaseDecorator();

        protected Dictionary<string, Func<BusinessBase>> creators = new Dictionary<string, Func<BusinessBase>>();
        protected Dictionary<string, Func<BusinessBaseDecorator>> decorators = new Dictionary<string, Func<BusinessBaseDecorator>>();
        private ConcurrentDictionary<string, Lazy<BusinessBaseDecorator>>
            decoratorsCreators = new ConcurrentDictionary<string, Lazy<BusinessBaseDecorator>>();
        public static BusinessBaseProvider Instance { get; private set; }
        private static IHttpContextAccessor HttpContextAccessor = null;
        private static ConnectionMultiplexer TheCache = null;
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
                return HttpContextAccessor?.HttpContext;
            }
        }

        public static string GetDBTableFor(string objectName)
        {
            if (ObjectToDBTable.Keys.Contains(objectName))
            {
                return ObjectToDBTable[objectName];
            }

            return objectName;
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
                obj = DefaultBusinessBase(objectName, dbNumber);
            }

            return obj;
        }

        public bool IsDecoratorCreated(string name, int dbNumber = 0)
        {
            return GetLazyDecorator(name, dbNumber).IsValueCreated;
        }

        public BusinessBaseDecorator GetDecorator(string name, int dbNumber = 0)
        {
            return GetLazyDecorator(name, dbNumber).Value;
        }

        private Lazy<BusinessBaseDecorator> GetLazyDecorator(string name, int dbNumber = 0)
        {
            return decoratorsCreators.GetOrAdd(
                name,
                new Lazy<BusinessBaseDecorator>(
                    () => GetDecoratorInternal(name, dbNumber),
                    LazyThreadSafetyMode.ExecutionAndPublication
                ));
        }

        private BusinessBaseDecorator GetDecoratorInternal(string objectName, int dbNumber)
        {
            BusinessBaseDecorator decorator;

            if (decorators.ContainsKey(objectName))
            {
                decorator = decorators[objectName].Invoke();
            }
            else
            {
                decorator = DefaultBusinessBaseDecorator();
            }
            decorator.SetProperties(objectName, dbNumber);

            return decorator;
        }

        public FilterBase GetFilter(HttpContext context, string objectName, string filterName = "")
        {
            AppUser user = AppUser.GetAppUser(context);
            FilterBase filter = GetDecorator(objectName, 0).GetFilter(filterName);
            int dbNumber = filter.Decorator.DBNumber;
            string filterKey = FilterKey(user, objectName, filterName, filter.Decorator.DBNumber);
            object objTemp;
            byte[] data;

            if (context.Items.TryGetValue(filterKey, out objTemp))
            {
                filter = (FilterBase)objTemp;
            }
            else
            {
                data = GetData(filterKey);

                if (data != null)
                {
                    try
                    {
                        filter.Deserialize(data);
                    }
                    catch (Exception exp)
                    {
                        // Sometimes Redis returns bad data.
                        int t = 2;
                    }
                }
                context.Items[filterKey] = filter;
            }

            return filter;
        }

        public static void StoreFilter(HttpContext context, FilterBase filter, string objectName, string filterName)
        {
            AppUser user = AppUser.GetAppUser(context);
            int dbNumber = filter.Decorator.DBNumber;
            string filterKey = FilterKey(user, objectName, filterName, filter.Decorator.DBNumber);

            StoreData(filterKey, filter.Serialize());
        }

        private static string FilterKey(AppUser user, string objectName, string filterName, int dbNumber)
        {
            return "F_" + objectName + "_" + filterName + "_" + dbNumber + "_" + user.Key;
        }

        private static string ObjectKey(string objectName, int dbNumber, string key)
        {
            return "O_" + objectName + "_" + dbNumber + "_" + key;
        }

        public static void StoreObject(BusinessBase obj, string objectName)
        {
            string objectKey = ObjectKey(objectName, obj.Decorator.DBNumber, obj.Key);

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
                    catch (Exception exp)
                    {
                        // Sometimes Redis returns bad data.
                        int t = 2;
                    }
                }
                if (readFromDB)
                {
                    if (key != "0" && key[0] != '-')
                    {
                        obj.ReadFromDB(key);
                    }
                    else
                    {
                        if (!obj.IsNew)
                        {
                            obj.SetNew();
                            objectKey = obj.Key;
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
