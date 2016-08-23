using AweShur.Core.DataViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public class FilterBase : IDataViewSetter
    {
        private Lazy<DB> lazyDB;

        public bool EmptyWhereReturnsEmpty { get; set; } = false;
        public string FastSearch { get; set; } = "";
        public bool FastSearchActivated { get; set; } = true;

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

        protected string Where(DataView dataView)
        {
            string where = "";

            if (FastSearchActivated && FastSearch != "")
            {
                foreach(DataViewColumn col in dataView.VisibleColumns)
                {
                    if (where != "")
                    {
                        where += " OR ";
                    }

                    where += col.FieldName + " LIKE '%" + FastSearch + "%'";
                }
            }

            return where;
        }

        public virtual IEnumerable<dynamic> Get(int order, SortDirection sortDirection,
            int fromRecord, int rowCount)
        {
            DataView dataView = new DataView(this);
            string where = Where(dataView);

            if (where == "" && EmptyWhereReturnsEmpty)
            {
                where = "1 = 0";
            }

            return dataView.Get(where, order, sortDirection, fromRecord, rowCount);
        }

        public virtual void SetDataView(DataView dataView, string elementName)
        {
            Tuple<Dictionary<string, DataViewColumn>, string> colsAndFrom;
             
            dataView.CurrentDB = CurrentDB;

            colsAndFrom = Decorator.GetFilterColumnsAndFromClause(elementName);

            dataView.Columns = colsAndFrom.Item1;
            dataView.FromClause = colsAndFrom.Item2;
        }
    }
}
