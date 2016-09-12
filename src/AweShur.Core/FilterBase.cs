using AweShur.Core.DataViews;
using Dapper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public class FilterBase : IDataViewSetter
    {
        private Lazy<DB> lazyDB;

        public bool EmptyWhereReturnsEmpty { get; set; } = false;
        public string FastSearch { get; set; } = "";
        public bool FastSearchActivated { get; set; } = true;
        public Dictionary<string, string> Filter { get; set; } = null;
        public int topRecords { get; set; } = 100;

        public BusinessBaseDecorator Decorator { get; }

        public FilterBase(BusinessBaseDecorator decorator, int dbNumber = 0)
        {
            Decorator = decorator;

            lazyDB = new Lazy<DB>(() => DB.InstanceNumber(dbNumber));
        }

        protected DB CurrentDB
        {
            get
            {
                return lazyDB.Value;
            }
        }

        public void Clear()
        {
            for (int index = 0; index < Filter.Count; ++index)
            {
                string key = Filter.Keys.ElementAt(index);
                PropertyDefinition prop;
                string fieldName = key;

                if (fieldName.Contains('|'))
                {
                    string[] parts = fieldName.Split('|');

                    fieldName = parts[0];
                }

                if (Decorator.Properties.TryGetValue(fieldName, out prop))
                {
                    Filter[key] = prop.DefaultSearch;
                }
            }
        }

        protected virtual Tuple<string, DynamicParameters> Where(DataView dataView)
        {
            string where = "";
            DynamicParameters param = new DynamicParameters();

            if (FastSearchActivated && FastSearch != "")
            {
                foreach(DataViewColumn col in dataView.VisibleColumns)
                {
                    if (where != "")
                    {
                        where += " OR ";
                    }

                    where += col.Expression + " LIKE @" + col.As;
                    param.Add(col.As, "%" + FastSearch + "%");
                }
            }
            else
            {
                foreach(KeyValuePair<string, string> item in Filter)
                {
                    PropertyDefinition prop;
                    string fieldName = item.Key;
                    string operation = "";

                    if (fieldName.Contains('|'))
                    {
                        string[] parts = fieldName.Split('|');

                        fieldName = parts[0];
                        operation = parts[1];
                    }
                    
                    if (Decorator.Properties.TryGetValue(fieldName, out prop))
                    {
                        prop.Where(ref where, ref param, item.Value, operation);
                    }
                }

                where = where.Replace("[TABLENAME]", Decorator.TableNameEncapsulated);
            }

            return new Tuple<string, DynamicParameters>(where, param);
        }

        public static Tuple<string, DynamicParameters> emptyWhere 
            = new Tuple<string, DynamicParameters>("1 = 0", null);

        public virtual IEnumerable<dynamic> Get(int order, SortDirection sortDirection,
            int fromRecord, int toRecord)
        {
            DataView dataView = new DataView(this);
            Tuple<string, DynamicParameters> where = Where(dataView);

            if (where.Item1 == "" && EmptyWhereReturnsEmpty)
            {
                where = emptyWhere;
            }

            return dataView.Get(where.Item1, where.Item2, order, sortDirection, fromRecord, topRecords);
        }

        public DataView GetEmpty()
        {
            return new DataView(this);
        }

        public virtual void SetDataView(DataView dataView)
        {
            dataView.CurrentDB = CurrentDB;

            dataView.Columns = new List<DataViewColumn>(2);
            dataView.Columns.Add(new DataViewColumn(Decorator.TableNameEncapsulated,
                Decorator.ListProperties[0]));
            if (Decorator.FirstStringProperty != null)
            {
                dataView.Columns.Add(new DataViewColumn(Decorator.TableNameEncapsulated,
                    Decorator.FirstStringProperty));
            }

            dataView.FromClause = Decorator.TableNameEncapsulated;
        }

        public virtual byte[] Serialize()
        {
            JObject obj = ToJObject();

            return Encoding.Unicode.GetBytes(obj.ToString(Newtonsoft.Json.Formatting.None));
        }

        public JObject ToJObject()
        {
            JObject obj = new JObject();

            obj.Add("t", JToken.FromObject(topRecords));
            obj.Add("f", JToken.FromObject(Filter));

            return obj;
        }

        public virtual void Deserialize(byte[] data)
        {
            string json = Encoding.Unicode.GetString(data);
            JObject obj = JObject.Parse(json);

            FromJObject(obj);
        }

        public void FromJObject(JObject obj)
        {
            topRecords = obj["t"].ToObject<int>();
            Filter = obj["f"].ToObject<Dictionary<string, string>>();
        }
    }
}
