using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweShur.Core.DataViews
{
    public interface IDataViewSetter
    {
        void SetDataView(DataView dataView);

        string GetFinalSQLQuery(DataView dataView, string whereClause, object param, int order, SortDirection sortDirection,
            int fromRecord, int rowCount);
    }

    public class DataView
    {
        public List<DataViewColumn> Columns { get; set; }
        public string FromClause { get; set; } = "";
        public string GroupByClause { get; set; } = "";
        public string PreOrderBy { get; set; } = "";
        public string PreWhere { get; set; } = "";
        public string PostOrderBy { get; set; } = "";

        private DB currentDB = null;

        public List<DataViewColumn> visibleColumns;
        private string selectNamedColumns = "";
        private string selectColumns = "";
        public string query = "";
        public string firstOrderBy = "";
        protected IDataViewSetter setter;

        public DataView(IDataViewSetter setter)
        {
            this.setter = setter;
            this.setter.SetDataView(this);

            InternalSet();
        }

        public DB CurrentDB
        {
            set
            {
                currentDB = value;
            }
        }

        public List<DataViewColumn> VisibleColumns
        {
            get
            {
                return visibleColumns;
            }
        }

        private void InternalSet()
        {
            int order = 0;
            int index = 0;

            visibleColumns = new List<DataViewColumn>(Columns.Count);

            foreach (DataViewColumn gc in Columns)
            {
                if (gc.IsID)
                {
                    gc.As = "ID";
                }
                else
                {
                    gc.As = "C" + index;
                }
                index++;

                if (gc.Visible)
                {
                    visibleColumns.Add(gc);

                    if (selectColumns != "")
                    {
                        selectColumns += ",";
                        selectNamedColumns += ",";
                    }
                    selectColumns += gc.Expression + " As " + gc.As;
                    selectNamedColumns += gc.As;

                    if (!gc.IsID && !gc.Hidden && firstOrderBy == "" && gc.OrderBy != "")
                    {
                        firstOrderBy = gc.OrderBy;
                    }

                    order++;
                }
            }

            if (currentDB == null)
            {
                throw new Exception("CurrentDB must be set.");
            }
            if (FromClause == null)
            {
                throw new Exception("FromClause must be set.");
            }
            if (selectColumns == null)
            {
                throw new Exception("No columns to show.");
            }

            // {SelectColumns} {FromClause} {WhereClause} {OrderBy} {FromRecord} {RowCount}
            query = currentDB.Dialect.GetFromToListSql.Replace("{SelectColumns}", selectColumns)
                .Replace("{SelectNamedColumns}", selectNamedColumns)
                .Replace("{GroupByClause}", (GroupByClause == "" ? "" : "GROUP BY " + GroupByClause))
                .Replace("{FromClause}", FromClause);
        }

        public IEnumerable<dynamic> Get(string whereClause, object param, int order, SortDirection sortDirection, 
            int fromRecord, int rowCount)
        {
            string sql = setter.GetFinalSQLQuery(this, whereClause, param, order, sortDirection, fromRecord, rowCount);

            return currentDB.Query(sql, param);
        }
    }
}
