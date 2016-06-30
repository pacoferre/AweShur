using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweShur.Core.Lists
{
    public class ListTable
    {
        private string sqlList = "";
        private int dbNumber = 0;
        public string[] Names { get; private set; } = null;
        public string ListName { get; private set; } = "";
        public int Count { get; private set; }
        public List<object[]> Items { get; private set; }
        public object[] ZeroItem { get; private set; }
        private Lazy<List<ListItemRest>> generator;
        private object pending = true;

        public ListTable(string listName, byte[] data)
        {
            ListName = listName;

            Deserialize(data);

            CreateGenerator();
        }

        public ListTable(string listName, string sql, int dbNumber, string allDescription = "All")
        {
            Count = 0;  //rows.Count en Dapper do foreach
            ZeroItem = new object[] { "0", allDescription };

            sqlList = sql;
            this.dbNumber = dbNumber;

            Items = new List<object[]>(40);
        }

        private void CreateGenerator()
        {
            generator = new Lazy<List<ListItemRest>>(() =>
            {
                List<ListItemRest> list = new List<ListItemRest>(Count);

                list.Add(new ListItemRest(ZeroItem[0].ToString(), ZeroItem[1].ToString()));

                foreach (object[] item in Items)
                {
                    list.Add(new ListItemRest(item[0].ToString(), item[1].ToString()));
                }

                return list;
            });
        }

        public void Invalidate()
        {
            lock (pending)
            {
                Items.Clear();

                pending = true;

                CreateGenerator();
            }
        }

        private void EnsureList()
        {
            if ((bool)pending)
            {
                lock(pending)
                {
                    if ((bool)pending)
                    {
                        IEnumerable<dynamic> dbItems = DB.InstanceNumber(dbNumber).Query(sqlList);

                        Items.Clear();

                        foreach (dynamic item in dbItems)
                        {
                            if (Names == null)
                            {
                                Names = item.Keys.ToArray();
                            }

                            Items.Add(item.Values.ToArray());
                        }

                        pending = false;
                    }
                }
            }
        }

        public List<ListItemRest> ToClient
        {
            get
            {
                return generator.Value;
            }
        }

        public object[] First
        {
            get
            {
                EnsureList();

                return Items[0];
            }
        }

        public JObject ToJSON()
        {
            JObject obj = new JObject();

            obj.Add("sqlList", JToken.FromObject(sqlList));
            obj.Add("dbNumber", JToken.FromObject(dbNumber));
            obj.Add("Names", JToken.FromObject(Names));
            obj.Add("Count", JToken.FromObject(Count));
            obj.Add("ZeroItem", JToken.FromObject(ZeroItem));
            obj.Add("Items", JToken.FromObject(Items));
            obj.Add("pending", JToken.FromObject((bool)pending ? "1" : ""));

            return obj;
        }

        public byte[] Serialize()
        {
            JObject obj = ToJSON();

            return Encoding.Unicode.GetBytes(obj.ToString(Newtonsoft.Json.Formatting.None));
        }

        public void Deserialize(byte[] data)
        {
            string json = Encoding.Unicode.GetString(data);
            JObject obj = JObject.Parse(json);

            sqlList = obj["sqlList"].ToObject(typeof(string)).ToString();
            dbNumber = (int)obj["dbNumber"].ToObject(typeof(int));
            Names = (string[])obj["Names"].ToObject(typeof(string[]));
            Count = (int)obj["IsModified"].ToObject(typeof(int));
            ZeroItem = (object[]) obj["IsDeleting"].ToObject(typeof(object[]));
            Items = (List<object[]>)obj["IsDeleting"].ToObject(typeof(List<object[]>));
            pending = obj["pending"].ToObject(typeof(string)).ToString() == "1";
        }
    }

    public class ListItemRest
    {
        public string i { get; set; } = "";
        public string t { get; set; } = "";

        public ListItemRest(string id, string text)
        {
            i = id;
            t = text;
        }
    }
}
