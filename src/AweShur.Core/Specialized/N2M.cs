using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.Specialized
{
    public class N2MDecorator : BusinessBaseDecorator
    {
        public string externalFieldNameM { get; set; }

        protected override void SetCustomProperties()
        {
            PropertyDefinition external = new PropertyDefinition(externalFieldNameM, externalFieldNameM, typeof(Int32));
            PropertyDefinition active = new PropertyDefinition("Active", "Active", typeof(bool), PropertyInputType.checkbox);

            Properties.Add(externalFieldNameM, external);
            Properties.Add("Active", active);

            Properties.Values.ElementAt(0).NoChecking = true;
            Properties.Values.ElementAt(1).NoChecking = true;

            base.SetCustomProperties();
        }
    }

    public class N2M : BusinessBase
    {
        protected string externalFieldNameM = "";

        protected string[] opcCampoOtros;

        public N2M(string tableName) : base(tableName)
        {
        }

        public override string GenerateKey(object[] dataItemValues)
        {
            return this[externalFieldNameM].ToString();
        }

        public string ExternalFieldNameM
        {
            get
            {
                return externalFieldNameM;
            }
        }

        public override void StoreToDB()
        {
            string ownFieldNameN = Decorator.ListProperties[0].FieldName;
            string ownFieldNameM = Decorator.ListProperties[1].FieldName;
            int externalID = (int)base[externalFieldNameM] != 0 ? (int)base[externalFieldNameM]
                : (int)base[ownFieldNameM];
            bool active = (bool)this["Active"];
            string sqlExists = "Select count(*) From " + Decorator.TableNameEncapsulated
                + " WHERE " + Decorator.ListProperties[0].FieldName + " = " + Parent.Parent.Key
                + " AND " + Decorator.ListProperties[1].FieldName + " = " + externalID;
            bool exists = DB.Instance.QueryFirstOrDefault<int>(sqlExists) != 0;

            if (Parent.Parent.IsDeleting || !active)
            {
                if (exists)
                {
                    base[ownFieldNameN] = Parent.Parent.Key.NoNullInt();
                    base[ownFieldNameM] = externalID;

                    if (!IsDeleting)
                    {
                        IsDeleting = true;
                    }

                    base.StoreToDB();
                }
            }
            else
            {
                if (!exists && !IsNew)
                {
                    SetNew();
                }

                base[ownFieldNameN] = Parent.Parent.Key.NoNullInt();
                base[ownFieldNameM] = externalID;

                base.StoreToDB();
            }
        }

        public override void SetNew(bool preserve = false, bool withoutCollections = false)
        {
            base.SetNew(true, true);
        }

        public override bool MatchFilter(string filterName)
        {
            return ((bool)this["Active"]);
        }

        public override bool ValidateDataItem(DataItem dataItem, ref string lastErrorMessage, ref string lastErrorProperty)
        {
            if ((bool)this["Active"])
            {
                return base.ValidateDataItem(dataItem, ref lastErrorMessage, ref lastErrorProperty);
            }

            return true;
        }
    }
}
