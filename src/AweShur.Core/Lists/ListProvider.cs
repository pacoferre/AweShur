using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AweShur.Core.Lists
{
    public class ListProvider
    {
        private ConcurrentDictionary<Tuple<string, string>, Lazy<ListTable>>
            listProviders = new ConcurrentDictionary<Tuple<string, string>, Lazy<ListTable>>();

        public ListTable GetList(string objectName, string listName)
        {
            Lazy<ListTable> lazy = listProviders.GetOrAdd(
                new Tuple<string, string>(objectName, listName),
                new Lazy<ListTable>(
                    () => GetListInternal(objectName, listName),
                    LazyThreadSafetyMode.ExecutionAndPublication
                ));

            return lazy.Value;
        }

        public void Invalidate(string objectName)
        {
            if (BusinessBaseProvider.ExistsData(Key(objectName, "")))
            {
                GetListInternal(objectName, "").Invalidate();
            }

            foreach(var kp in listProviders)
            {
                if (kp.Key.Item1 == objectName)
                {
                    if (kp.Value.IsValueCreated)
                    {
                        kp.Value.Value.Invalidate();
                    }
                }
            }
        }

        private string Key(string objectName, string listName)
        {
            return "list_" + objectName + "_" + listName;
        }

        private ListTable GetListInternal(string objectName, string listName)
        {
            string key = Key(objectName, listName);
            ListTable list;
            byte[] listData = BusinessBaseProvider.GetData(key);

            if (listData == null)
            {
                BusinessBaseDefinition def;

                def = BusinessBaseProvider.Instance.GetDefinition(objectName);

                list = def.GetList(listName, def.DBNumber);

                BusinessBaseProvider.StoreData(key, list.Serialize());
            }
            else
            {
                list = new ListTable(listName, listData);
            }

            return list;
        }
    }
}
