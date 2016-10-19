using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.DataViews
{
    public class DataViewColumn
    {
        public BasicType BasicType { get; set; } = BasicType.Text;
        public bool IsID { get; set; } = false;
        public string OrderBy { get; set; } = "";
        public HorizontalAlign Align { get; set; } = HorizontalAlign.Left;
        public string Label { get; set; } = "";
        public string Expression { get; set; } = "";
        public string As { get; set; } = "";
        public string Format { get; set; } = "";
        public bool Visible { get; set; } = true;
        public bool Hidden { get; set; } = false;
        public bool Hidable { get; set; } = false;
        public bool Resizable { get; set; } = false;
        public bool FastSearchColumn { get; set; } = false;
        public bool Money { get; set; } = false;
        public int MinWidth { get; set; } = 0;
        public int MaxWidth { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Flex { get; set; } = 0;
        public string CustomRenderer { get; set; } = "";

        public DataViewColumn(string tableNameEncapsulated, PropertyDefinition property)
        {
            BasicType = property.BasicType;
            IsID = property.IsPrimaryKey;
            Hidden = property.IsIdentity || property.IsPrimaryKey;
            Expression = tableNameEncapsulated + "." + property.FieldName;
            Label = property.Label;

            if (property.BasicType == BasicType.Number)
            {
                Align = HorizontalAlign.Right;
            }

            if (!Hidden)
            {
                OrderBy = tableNameEncapsulated + "." + property.FieldName;
            }

            if (property.BasicType == BasicType.Text)
            {
                FastSearchColumn = true;
            }
        }

        public DataViewColumn(string expression, string label)
        {
            Expression = expression;
            Label = label;
            OrderBy = expression;
        }
    }
}
