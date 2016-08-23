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
        private ConcurrentDictionary<Tuple<string, string, string>, Lazy<ListTable>>
            listProviders = new ConcurrentDictionary<Tuple<string, string, string>, Lazy<ListTable>>();

        public ListTable GetList(string objectName, string listName = "", string parameter = "")
        {
            if (listName == "")
            {
                listName = objectName;
            }

            Lazy<ListTable> lazy = listProviders.GetOrAdd(
                new Tuple<string, string, string>(objectName, listName, parameter),
                new Lazy<ListTable>(
                    () => GetListInternal(objectName, listName, parameter),
                    LazyThreadSafetyMode.ExecutionAndPublication
                ));

            return lazy.Value;
        }

        public void Invalidate(string objectName)
        {
            if (BusinessBaseProvider.ExistsData(Key(objectName, "", "")))
            {
                GetListInternal(objectName, "", "").Invalidate();
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

        private string Key(string objectName, string listName, string parameter)
        {
            return "list_" + objectName + "_" + listName + "_" + parameter;
        }

        private ListTable GetListInternal(string objectName, string listName, string parameter)
        {
            string key = Key(objectName, listName, parameter);
            ListTable list;
            byte[] listData = BusinessBaseProvider.GetData(key);

            if (listData == null)
            {
                BusinessBaseDefinition def;

                def = BusinessBaseProvider.Instance.GetDefinition(objectName);

                list = def.GetList(listName, parameter, def.DBNumber);

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
