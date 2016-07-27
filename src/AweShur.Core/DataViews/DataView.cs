using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweShur.Core.DataViews
{
    public interface IDataViewSetter
    {
        void SetDataView(DataView dataView, string elementName);
    }

    public class DataView
    {
        public Dictionary<string, DataViewColumn> Columns { get; set; }
        public string ElementName { get; } = "";
        public string FromClause { get; set; } = "";
        public string PreOrderBy { get; set; } = "";

        private DB currentDB = null;

        private List<DataViewColumn> visibleColumns;
        private string selectNamedColumns = "";
        private string selectColumns = "";
        private string query = "";
        private string firstOrderBy = "";

        public DataView(IDataViewSetter setter, string elementName = "")
        {
            ElementName = elementName;
            setter.SetDataView(this, elementName);

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

            visibleColumns = new List<DataViewColumn>(Columns.Count);

            foreach (KeyValuePair<string, DataViewColumn> gc in Columns)
            {
                if (gc.Value.Visible)
                {
                    visibleColumns.Add(gc.Value);

                    if (selectColumns != "")
                    {
                        selectColumns += ",";
                        selectNamedColumns += ",";
                    }
                    selectColumns += gc.Value.FieldName + " As " + gc.Key;
                    selectNamedColumns += gc.Key;

                    if (firstOrderBy == "" && gc.Value.OrderBy != "")
                    {
                        firstOrderBy = gc.Value.OrderBy;
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
                .Replace("{FromClause}", FromClause);
        }

        public IEnumerable<dynamic> Get(string whereClause, int order, SortDirection sortDirection, 
            int fromRecord, int rowCount)
        {
            // {SelectColumns} {FromClause} {WhereClause} {OrderBy} {FromRecord} {RowCount}
            string sql = query;
            string orderBy = firstOrderBy;

            if (visibleColumns[order].OrderBy != "")
            {
                orderBy = visibleColumns[order].OrderBy;
            }

            if (PreOrderBy != "")
            {
                orderBy = PreOrderBy + (orderBy == "" ? "" : ", " + orderBy);
            }

            if (sortDirection == SortDirection.Descending)
            {
                if (orderBy != "")
                {
                    orderBy = orderBy.Replace(",", " DESC ,");
                    orderBy += " DESC";
                }
            }

            sql = sql.Replace("{OrderBy}", orderBy)
                .Replace("{WhereClause}", (whereClause == "" ? "" : " WHERE " + whereClause))
                .Replace("{FromRecord}", fromRecord.ToString())
                .Replace("{RowCount}", rowCount.ToString());

            return currentDB.Query(sql);
        }
    }
}
