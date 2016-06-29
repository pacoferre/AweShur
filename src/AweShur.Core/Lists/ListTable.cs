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
        public string[] Names { get; private set; } = null;
        public string ListName { get; private set; } = "";
        public int Count { get; private set; }
        public List<object[]> Items { get; private set; }
        public object[] ZeroItem { get; private set; }

        public ListTable(string listName, byte[] data = null)
        {
            ListName = listName;
        }

        public ListTable(string listName, IEnumerable<dynamic> items, string allDescription = "All")
        {
            Count = 0;  //rows.Count en Dapper do foreach
            Items = new List<object[]>(40);

            foreach (dynamic item in items)
            {
                if (Names == null)
                {
                    Names = item.Keys.ToArray();
                }

                Items.Add(item.Values.ToArray());
            }

            ZeroItem = new object[] { 0, allDescription };
        }

        public object[] First
        {
            get
            {
                return Items[0];
            }
        }

        public JObject ToJSON()
        {
            JObject obj = new JObject();

            obj.Add("ListName", JToken.FromObject(ListName));
            obj.Add("Names", JToken.FromObject(Names));
            obj.Add("Count", JToken.FromObject(Count));
            obj.Add("ZeroItem", JToken.FromObject(ZeroItem));
            obj.Add("Items", JToken.FromObject(Items));

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

            ListName = obj["ListName"].ToObject(typeof(string)).ToString();
            Names = (string[])obj["Names"].ToObject(typeof(string[]));
            Count = (int)obj["IsModified"].ToObject(typeof(int));
            ZeroItem = (object[]) obj["IsDeleting"].ToObject(typeof(object[]));
            Items = (List<object[]>)obj["IsDeleting"].ToObject(typeof(List<object[]>));
        }
    }
}
