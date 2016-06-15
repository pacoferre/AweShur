using System;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public class BusinessBaseProvider
    {
        private Dictionary<string, Func<BusinessBase>> creators = new Dictionary<string, Func<BusinessBase>>();
        private Dictionary<string, Func<BusinessBaseDefinition>> decorators = new Dictionary<string, Func<BusinessBaseDefinition>>();
        private ConcurrentDictionary<string, Lazy<BusinessBaseDefinition>>
            definitionsCache = new ConcurrentDictionary<string, Lazy<BusinessBaseDefinition>>();
        public static BusinessBaseProvider Instance { get; set; }

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
    }
}
