﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.DataViews
{
    public class DataViewColumn
    {
        public bool IsID { get; set; } = false;
        public string OrderBy { get; set; } = "";
        public HorizontalAlign Align { get; set; } = HorizontalAlign.Left;
        public string Label { get; set; } = "";
        public string FieldName { get; set; } = "";
        public string Width { get; set; } = "";
        public string Format { get; set; } = "";
        // Used only in Select clause, for pre-ordering or something else
        public bool Visible { get; set; } = true;
        public bool Hidden { get; set; } = false;
        public bool FastSearchColumn { get; set; } = false;

        public DataViewColumn()
        {

        }

        public DataViewColumn(PropertyDefinition property)
        {
            IsID = property.IsPrimaryKey;
            Hidden = property.IsIdentity || property.IsPrimaryKey;
            FieldName = property.FieldName;
            Label = property.Label;

            if (!Hidden)
            {
                OrderBy = property.FieldName;
            }

            if (property.BasicType == BasicType.Text)
            {
                FastSearchColumn = true;
            }
        }
    }
}