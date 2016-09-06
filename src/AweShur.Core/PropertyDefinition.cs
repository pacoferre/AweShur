using AweShur.Core.Lists;
using AweShur.Core.Security;
using Dapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public enum BasicType
    {
        Number = 0,
        Text = 1,
        DateTime = 2,
        TextLong = 3,
        Bit = 4,
        Bynary = 5,
        GUID = 6,
        Other = 7
    }

    public enum PropertyInputType
    {
        text,
        password,
        radio,
        date,
        email,
        textarea,
        number,
        checkbox,
        time,
        datetimeHHmm,
        datetimeHHmmss,
        tel,
        range,
        select
    }

    public class PropertyDefinition
    {
        public static string EmptyErrorMessage = "{0} empty";
        public static string EmptySelectionErrorMessage = "{0} selection missing";
        public static string EmptyValueErrorMessage = "{0} value missing";

        public BasicType BasicType { get; } = BasicType.Text;
        private PropertyInputType inputType = PropertyInputType.text;

        public int Index { get; internal set; } = 0;
        public bool IsDBField { get; } = false;

        public bool IsNullable { get; } = false;
        public bool IsComputed { get; } = false;
        public bool IsIdentity { get; } = false;
        public bool IsPrimaryKey { get; } = false;
        public bool IsReadOnly { get; set; } = false;
        public bool IsOnlyOnNew { get; set; } = false;
        public bool SetModified { get; set; } = true;
        public string DBDataType { get; } = "";
        public string FieldName { get; } = "";
        public Type DataType { get; set; }

        public string Label { get; set; } = "";
        public bool LabelIsFieldName { get; set; } = false;
        public string Format { get; set; } = "";
        public string Pattern { get; set; } = "";
        public int MaxLength { get; set; } = 0;
        public bool Required { get; set; } = false;
        public string RequiredErrorMessage { get; set; } = "";
        public bool NoLabelRequired { get; set; } = false;
        public bool NoChecking { get; set; } = false;
        public string Min { get; set; } = "";
        public string Max { get; set; } = "";
        public string Step { get; set; } = "";
        public string ListObjectName { get; set; } = "";
        public string ListName { get; set; } = "";
        public bool IsObjectView { get; set; } = false;
        public bool ListAjax { get; set; } = false;
        public int Rows { get; set; } = 0;
        public string DefaultSearch { get; set; } = "";
        public bool SearchMultipleSelect { get; set; } = true;
        public object DefaultValue { get; set; } = null;
        public bool AlwaysFloatLabel { get; set; } = true;

        public PropertyDefinition(DBDialect.ColumnDefinition colDef)
        {
            IsDBField = true;
            IsNullable = colDef.IsNullable;
            if (!IsNullable)
            {
                Required = true;
            }

            IsComputed = colDef.IsComputed;
            IsIdentity = colDef.IsIdentity;
            IsPrimaryKey = colDef.IsPrimaryKey;
            if (IsComputed)
            {
                IsReadOnly = true;
            }
            if (IsComputed || IsIdentity)
            {
                NoChecking = true;
            }
            DBDataType = colDef.DBDataType;
            FieldName = colDef.ColumnName;
            DataType = DBDialect.GetColumnType(colDef.DBDataType);
            BasicType = DBDialect.GetBasicType(DataType);
            MaxLength = colDef.MaxLength;

            if (BasicType == BasicType.Text && MaxLength == 10000)
            {
                BasicType = BasicType.TextLong;
            }
            if (BasicType == BasicType.Bit && !IsNullable)
            {
                DefaultValue = false;
            }

            if (BasicType == BasicType.Bit)
            {
                inputType = PropertyInputType.checkbox;
            }
            else if (BasicType == BasicType.DateTime)
            {
                Type = PropertyInputType.date;
            }

            if (FieldName.Length > 1)
            {
                Label = FieldName[0].ToString().ToUpper() + FieldName.Substring(1);
            }
        }

        public PropertyDefinition(string propertyName, string label, System.Type dataType, 
            PropertyInputType type = PropertyInputType.text, bool isPrimaryKey = false)
        {
            FieldName = propertyName;
            Label = label;

            DataType = dataType;
            BasicType = DBDialect.GetBasicType(DataType);

            if (Label == "")
            {
                NoLabelRequired = true;
            }
            Type = type;
            if (Type == PropertyInputType.text)
            {
                this.MaxLength = 90;
            }

            IsPrimaryKey = isPrimaryKey;
        }

        public string InputType
        {
            get
            {
                if (inputType == PropertyInputType.datetimeHHmm || inputType == PropertyInputType.datetimeHHmmss
                    || inputType == PropertyInputType.date || inputType == PropertyInputType.number)
                {
                    return "text";
                }

                return inputType.ToString();
            }
        }

        public PropertyInputType Type
        {
            get
            {
                return inputType;
            }
            set
            {
                this.inputType = value;

                if (value == PropertyInputType.date)
                {
                    this.Format = "{0:dd/MM/yyyy}";
                    this.MaxLength = 10;
                    this.Pattern = "day_month_year";
                }
                else if (value == PropertyInputType.datetimeHHmm)
                {
                    this.Format = "{0:dd/MM/yyyy HH:mm}";
                    this.MaxLength = 16;
                    this.Pattern = "day_month_year hour_minute";
                    this.Required = false;
                    this.NoLabelRequired = true;
                }
                if (value == PropertyInputType.datetimeHHmmss)
                {
                    this.Format = "{0:dd/MM/yyyy HH:mm:ss}";
                    this.MaxLength = 19;
                    this.Pattern = "day_month_year hour_minute_second";
                    this.Required = false;
                    this.NoLabelRequired = true;
                }
            }
        }

        public string GetValue(BusinessBase obj)
        {
            string value = "";

            if (Type == PropertyInputType.date)
            {
                if (!(obj[FieldName] == null))
                {
                    if (obj.IsReadOnly(FieldName))
                    {
                        value = String.Format(Format, (DateTime)obj[FieldName]);
                    }
                    else
                    {
                        value = ((DateTime)obj[FieldName]).ToString("yyyy/MM/dd");
                    }
                }
            }
            else if (Type == PropertyInputType.datetimeHHmm)
            {
                if (!(obj[FieldName] == null))
                {
                    if (obj.IsReadOnly(FieldName))
                    {
                        value = String.Format(Format, (DateTime)obj[FieldName]);
                    }
                    else
                    {
                        value = ((DateTime)obj[FieldName]).ToString("yyyy/MM/dd HH:mm");
                    }
                }
            }
            else if (Type == PropertyInputType.datetimeHHmmss)
            {
                if (!(obj[FieldName] == null))
                {
                    if (obj.IsReadOnly(FieldName))
                    {
                        value = String.Format(Format, (DateTime)obj[FieldName]);
                    }
                    else
                    {
                        value = ((DateTime)obj[FieldName]).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                }
            }
            else if (Type == PropertyInputType.checkbox || BasicType == BasicType.Bit)
            {
                value = obj[FieldName].NoNullBool() ? "1" : "0";
            }
            else if (Type == PropertyInputType.select)
            {
                if (obj[FieldName] == null)
                {
                    value = "0";
                }
                else
                {
                    value = obj[FieldName].ToString();
                }

                if (Required && value == "0" && !IsObjectView)
                {
                    // First element
                    ListTable dt = BusinessBaseProvider.ListProvider.GetList(ListObjectName, ListName);

                    if (dt.Items.Count > 0)
                    {
                        value = dt.First[0].ToString();
                    }
                }
            }
            else
            {
                if (Format != "")
                {
                    value = String.Format(Format, obj[FieldName]);
                }
                else
                {
                    value = obj[FieldName].NoNullString();
                }
            }

            return value;
        }

        public void SetValue(BusinessBase obj, string value)
        {
            if (obj.IsReadOnly(FieldName) || value == null)
            {
                return;
            }

            if (BasicType == BasicType.Text || BasicType == BasicType.TextLong)
            {
                value = value.NoNullString().Trim();

                if (IsNullable && (value == "" || (value == "0" && Type == PropertyInputType.select)))
                {
                    // strings as IDs !!! :)
                    if (!(obj[FieldName] == null))
                    {
                        obj[FieldName] = null;
                    }
                }
                else
                {
                    if (obj[FieldName].NoNullString() != value)
                    {
                        obj[FieldName] = value;
                    }
                }
            }

            else if (Type == PropertyInputType.checkbox || BasicType == BasicType.Bit)
            {
                if (obj[FieldName].NoNullBool() != (value == "1"))
                {
                    obj[FieldName] = (value == "1");
                }
            }

            else if (Type == PropertyInputType.datetimeHHmm || Type == PropertyInputType.datetimeHHmmss || Type == PropertyInputType.date)
            {
                try
                {
                    if (value != "")
                    {
                        AppUser us = obj.CurrentUser;
                        DateTime d;
                        string _format = "yyyy/MM/dd";

                        if (Type == PropertyInputType.datetimeHHmm)
                        {
                            _format += " HH:mm";
                        }
                        else if (Type == PropertyInputType.datetimeHHmmss)
                        {
                            _format += " HH:mm:ss";
                        }

                        try
                        {
                            d = DateTime.ParseExact(value, _format, us.Culture);
                        }
                        catch
                        {
                            d = DateTime.Parse(value, us.Culture);
                        }

                        if (obj[FieldName] == null)
                        {
                            obj[FieldName] = d;
                        }
                        else
                        {
                            if ((DateTime)obj[FieldName] != d)
                            {
                                obj[FieldName] = d;
                            }
                        }
                    }
                    else
                    {
                        if (!(obj[FieldName] == null))
                        {
                            obj[FieldName] = null;
                        }
                    }
                }
                catch
                {
                    if (!(obj[FieldName] == null))
                    {
                        obj[FieldName] = null;
                    }
                }
            }

            else if (BasicType == BasicType.GUID)
            {
                Guid newGuid = Guid.Empty;

                try
                {
                    newGuid = Guid.Parse(value.ToString());
                    if ((Guid)obj[FieldName] != newGuid)
                    {
                        obj[FieldName] = newGuid;
                    }
                }
                catch
                {
                }
            }

            else if (BasicType == BasicType.Number)
            {
                try
                {
                    Lib.Numerize(ref value);

                    if (obj[FieldName] == null && !IsNullable)
                    {
                        obj[FieldName] = 0;
                    }

                    if (IsNullable && (value == "" || (value == "0" && Type == PropertyInputType.select)))
                    {
                        if (!(obj[FieldName] == null))
                        {
                            obj[FieldName] = null;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (Double.Parse(value) == 0)
                            {
                                value = "0";
                            }
                        }
                        catch
                        {
                            value = "0";
                        }
                        if (obj[FieldName] == null && value != "")
                        {
                            obj[FieldName] = 0;
                        }

                        if (DataType == typeof(System.Int32))
                        {
                            int tmp = Int32.Parse(value);

                            if ((int)obj[FieldName] != tmp)
                            {
                                obj[FieldName] = tmp;
                            }
                        }
                        if (DataType == typeof(System.Int64))
                        {
                            long tmp = Int32.Parse(value);

                            if ((long)obj[FieldName] != tmp)
                            {
                                obj[FieldName] = tmp;
                            }
                        }
                        else if (DataType == typeof(System.Int16))
                        {
                            short tmp = Int16.Parse(value);

                            if ((short)obj[FieldName] != tmp)
                            {
                                obj[FieldName] = tmp;
                            }
                        }
                        if (DataType == typeof(System.Single))
                        {
                            Single tmp = Single.Parse(value);

                            if ((float)obj[FieldName] != tmp)
                            {
                                obj[FieldName] = tmp;
                            }
                        }
                        if (DataType == typeof(System.Double))
                        {
                            Double tmp = Double.Parse(value);

                            if ((double)obj[FieldName] != tmp)
                            {
                                obj[FieldName] = tmp;
                            }
                        }
                        if (DataType == typeof(System.Decimal))
                        {
                            Decimal tmp = Decimal.Parse(value);

                            if ((decimal)obj[FieldName] != tmp)
                            {
                                obj[FieldName] = tmp;
                            }
                        }
                    }
                }
                catch
                {
                    //Invalid user input
                    //value = "0";
                    //obj[PropertyName] = 0;
                }
            }
        }

        public bool Validate(object value, ref string lastErrorMessage, ref string lastErrorProperty)
        {
            if (!NoChecking)
            {
                if (!IsNullable && value == null)
                {
                    lastErrorMessage = Label + " empty";
                    lastErrorProperty = FieldName;

                    return false;
                }
                if (Required)
                {
                    if ((BasicType == BasicType.Text || BasicType == BasicType.TextLong) && value.NoNullString() == "")
                    {
                        lastErrorMessage = String.Format(EmptyErrorMessage, Label);
                        lastErrorProperty = FieldName;

                        return false;
                    }
                    if (BasicType == BasicType.Number && value.NoNullInt() == 0)
                    {
                        if (ListObjectName != "")
                        {
                            lastErrorMessage = String.Format(EmptySelectionErrorMessage, Label);
                        }
                        else
                        {
                            lastErrorMessage = String.Format(EmptyValueErrorMessage, Label);
                        }
                        lastErrorProperty = FieldName;

                        return false;
                    }
                }
            }

            return true;
        }

        public void Where(ref string where, ref DynamicParameters param, string value, string operation)
        {
            value = value.NoNullString().Trim();

            if (value != "")
            {
                object obj = null;

                if (BasicType == BasicType.Text || BasicType == BasicType.TextLong)
                {
                    obj = value;
                    if (operation == "")
                    {
                        operation = "LIKE";
                    }
                }
                else if (Type == PropertyInputType.checkbox || BasicType == BasicType.Bit)
                {
                    obj = (value == "1");
                }
                else if (Type == PropertyInputType.datetimeHHmm || Type == PropertyInputType.datetimeHHmmss || Type == PropertyInputType.date)
                {
                    try
                    {
                        if (value != "")
                        {
                            DateTime d;
                            string _format = "yyyy/MM/dd";

                            if (Type == PropertyInputType.datetimeHHmm)
                            {
                                _format += " HH:mm";
                            }
                            else if (Type == PropertyInputType.datetimeHHmmss)
                            {
                                _format += " HH:mm:ss";
                            }

                            try
                            {
                                d = DateTime.ParseExact(value, _format, null);
                            }
                            catch
                            {
                                d = DateTime.Parse(value);
                            }

                            obj = d;
                        }
                    }
                    catch
                    {
                    }
                }
                else if (Type == PropertyInputType.select && value != "0")
                {
                    if (operation == "")
                    {
                        operation = "IN";
                    }
                    string[] parts = value.Split(',');
                    List<int> numbers = new List<int>(parts.Length);

                    foreach(string part in parts)
                    {
                        int number = 0;

                        try
                        {
                            number = Int32.Parse(part);
                        }
                        catch
                        { }

                        if (number != 0)
                        {
                            numbers.Add(number);
                        }
                    }

                    if (numbers.Count > 0)
                    {
                        obj = numbers;
                    }
                }
                else if (BasicType == BasicType.Number && Type != PropertyInputType.select)
                {
                    try
                    {
                        Lib.Numerize(ref value);

                        if (DataType == typeof(System.Int32))
                        {
                            obj = Int32.Parse(value);
                        }
                        else if (DataType == typeof(System.Int16))
                        {
                            obj = Int16.Parse(value);
                        }
                        if (DataType == typeof(System.Single))
                        {
                            obj = Single.Parse(value);
                        }
                        if (DataType == typeof(System.Double))
                        {
                            obj = Double.Parse(value);
                        }
                        if (DataType == typeof(System.Decimal))
                        {
                            obj = Decimal.Parse(value);
                        }
                    }
                    catch
                    {
                    }
                }

                if (obj != null)
                {
                    if (where != "")
                    {
                        where += " AND ";
                    }
                    where += "[TABLENAME]." + FieldName + " " + operation + " @" + FieldName;
                    param.Add(FieldName, obj);
                }
            }
        }
    }
}
