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
        private ConcurrentDictionary<string, Lazy<ListTable>>
            listProviders = new ConcurrentDictionary<string, Lazy<ListTable>>();
        public Dictionary<string, Tuple<string, int>> listGenerators = new Dictionary<string, Tuple<string, int>>();

        public ListTable GetList(string listName)
        {
            Lazy<ListTable> lazy = listProviders.GetOrAdd(
                listName,
                new Lazy<ListTable>(
                    () => GetListInternal(listName),
                    LazyThreadSafetyMode.ExecutionAndPublication
                ));

            return lazy.Value;
        }

        private ListTable GetListInternal(string listName)
        {
            string key = "list_" + listName;
            ListTable list;
            byte[] listData = BusinessBaseProvider.GetData(key);

            if (listData == null)
            {
                string businessBaseDefinitionName = listName;
                int dbNumber = 0;
                BusinessBaseDefinition def;
                Tuple<string, int> listInto;

                if (listGenerators.TryGetValue(listName, out listInto))
                {
                    businessBaseDefinitionName = listInto.Item1;
                    dbNumber = listInto.Item2;
                }

                def = BusinessBaseProvider.Instance.GetDefinition(businessBaseDefinitionName, dbNumber);

                list = def.GetList(listName, dbNumber);

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
