using AweShur.Core.Lists;
using AweShur.Core.Security;
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
        public BasicType BasicType { get; } = BasicType.Text;
        private PropertyInputType inputType = PropertyInputType.text;

        public int Index { get; internal set; } = 0;
        public bool IsDBField { get; } = false;

        public bool IsNullable { get; } = false;
        public bool IsComputed { get; } = false;
        public bool IsIdentity { get; } = false;
        public bool IsPrimaryKey { get; } = false;
        public bool IsReadOnly { get; set; } = false;
        public string DBDataType { get; } = "";
        public string FieldName { get; } = "";
        public Type DataType { get; }

        public string Label { get; set; } = "";
        public string Format { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public string Pattern { get; set; } = "";
        public int MaxLength { get; set; } = 90;
        public bool Required { get; set; } = false;
        public bool NoLabelRequired { get; set; } = false;
        public bool NoChecking { get; set; } = false;
        public string Min { get; set; } = "";
        public string Max { get; set; } = "";
        public string Step { get; set; } = "";
        public string ListSource { get; set; } = "";
        public bool IsObjectView { get; set; } = false;
        public bool ListAjax { get; set; } = false;
        public int Rows { get; set; } = 2;
        public string DefaultSearch { get; set; } = "";
        public bool SearchMultipleSelect { get; set; } = true;
        public object DefaultValue { get; set; } = null;

        public PropertyDefinition(DBDialect.ColumnDefinition colDef)
        {
            IsDBField = true;
            IsNullable = colDef.IsNullable;
            IsComputed = colDef.IsComputed;
            IsIdentity = colDef.IsIdentity;
            IsPrimaryKey = colDef.IsPrimaryKey;
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
        }

        public PropertyDefinition(string propertyName, string label, BasicType basicType = BasicType.Text, PropertyInputType type = PropertyInputType.text)
        {
            FieldName = propertyName;
            Label = label;
            if (Label == "")
            {
                NoLabelRequired = true;
            }
            Type = type;
            if (Type == PropertyInputType.text)
            {
                this.MaxLength = 90;
            }
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
                    this.Format = "{0:dd-MM-yyyy}";
                    this.MaxLength = 10;
                    this.Pattern = "day_month_year";
                }
                else if (value == PropertyInputType.datetimeHHmm)
                {
                    this.Format = "{0:dd-MM-yyyy HH:mm}";
                    this.MaxLength = 16;
                    this.Pattern = "day_month_year hour_minute";
                    this.Required = false;
                    this.NoLabelRequired = true;
                }
                if (value == PropertyInputType.datetimeHHmmss)
                {
                    this.Format = "{0:dd-MM-yyyy HH:mm:ss}";
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
                    value = ((DateTime)obj[FieldName]).ToString(obj.CurrentUser.ShortDateFormat);
                }
            }
            else if (Type == PropertyInputType.datetimeHHmm)
            {
                if (!(obj[FieldName] == null))
                {
                    value = ((DateTime)obj[FieldName]).ToString(obj.CurrentUser.ShortDateFormat + " HH:mm");
                }
            }
            else if (Type == PropertyInputType.datetimeHHmmss)
            {
                if (!(obj[FieldName] == null))
                {
                    value = ((DateTime)obj[FieldName]).ToString(obj.CurrentUser.ShortDateFormat + " HH:mm:ss");
                }
            }
            else if (Type == PropertyInputType.checkbox)
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
                    ListTable dt = ListProvider.Instance(ListSource).GetList();

                    if (dt.Count > 0)
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
                    try
                    {
                        value = obj[FieldName].ToString();
                    }
                    catch (Exception exp)
                    {
                        int t = 2;
                    }
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

                if (obj[FieldName].NoNullString() != value)
                {
                    obj[FieldName] = value;
                }
            }

            else if (Type == PropertyInputType.checkbox)
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

                        try
                        {
                            d = DateTime.ParseExact(value, Format, us.Culture);
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
                        lastErrorMessage = Label + " empty";
                        lastErrorProperty = FieldName;

                        return false;
                    }
                    if (BasicType == BasicType.Number && value.NoNullInt() == 0)
                    {
                        if (ListSource != "")
                        {
                            lastErrorMessage = Label + " selection missing";
                        }
                        else
                        {
                            lastErrorMessage = Label + " value missing";
                        }
                        lastErrorProperty = FieldName;

                        return false;
                    }
                }
            }

            return true;
        }
    }
}
