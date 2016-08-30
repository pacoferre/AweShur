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
        private Dictionary<string, object> parameters = null;
        private int dbNumber = 0;
        public string[] Names { get; private set; } = new string[0];
        public string ListName { get; private set; } = "";
        public List<object[]> Items { get; private set; }
        public object[] ZeroItem { get; private set; }
        private Lazy<List<ListItemRest>> generator;
        private object pending = true;
        private DateTime lastReaded = DateTime.Now;

        public ListTable(string listName, byte[] data)
        {
            ListName = listName;

            Deserialize(data);

            CreateGenerator();
        }

        public ListTable(string listName, string sql, dynamic parameters, int dbNumber, string allDescription)
        {
            ZeroItem = new object[] { "0", allDescription };

            sqlList = sql;
            this.parameters = parameters;
            this.dbNumber = dbNumber;

            Items = new List<object[]>(40);

            CreateGenerator();
        }

        private void CreateGenerator()
        {
            generator = new Lazy<List<ListItemRest>>(() =>
            {
                List<ListItemRest> list = new List<ListItemRest>(Items.Count + 1);

                list.Add(new ListItemRest(ZeroItem[0].ToString(), ZeroItem[1].ToString()));

                EnsureList();

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
                        IEnumerable<dynamic> dbItems = DB.InstanceNumber(dbNumber).Query(sqlList, parameters);

                        Items.Clear();

                        foreach (IDictionary<string, object> item in dbItems)
                        {
                            if (Names.Length == 0)
                            {
                                Names = item.Keys.ToArray();
                            }

                            Items.Add(item.Values.ToArray());
                        }

                        pending = false;
                        lastReaded = DateTime.Now;
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

        public string GetValue(int id)
        {
            return Items.Find(item => item[0].NoNullInt() == id)[1].NoNullString();
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

            obj.Add("sql", JToken.FromObject(sqlList));

            obj["np"] = JToken.FromObject(parameters == null ? "1" : "");
            if (parameters != null)
            {
                obj.Add("pars", JToken.FromObject(parameters));
            }
            obj.Add("dbN", JToken.FromObject(dbNumber));
            obj.Add("Nam", JToken.FromObject(Names));
            obj.Add("ZI", JToken.FromObject(ZeroItem));
            obj.Add("It", JToken.FromObject(Items));
            obj.Add("pe", JToken.FromObject((bool)pending ? "1" : ""));
            obj.Add("lR", JToken.FromObject(lastReaded));

            if (lastReaded < DateTime.Now.AddMinutes(-5))
            {
                pending = true;
            }

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

            sqlList = obj["sql"].ToObject(typeof(string)).ToString();
            if (obj["np"].ToObject(typeof(string)).ToString() == "1")
            {
                parameters = null;
            }
            else
            {
                parameters = obj["pars"].ToObject<Dictionary<string, object>>();
            }
            dbNumber = (int)obj["dbNr"].ToObject(typeof(int));
            Names = (string[])obj["Nam"].ToObject(typeof(string[]));
            ZeroItem = (object[]) obj["ZI"].ToObject(typeof(object[]));
            Items = (List<object[]>)obj["It"].ToObject(typeof(List<object[]>));
            pending = obj["pe"].ToObject(typeof(string)).ToString() == "1";
            lastReaded = (DateTime)obj["lR"].ToObject(typeof(DateTime));
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
