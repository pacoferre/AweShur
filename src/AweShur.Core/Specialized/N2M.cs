using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.Specialized
{
    public class N2MDecorator : BusinessBaseDefinition
    {
        protected override void SetCustomProperties()
        {
            PropertyDefinition active = new PropertyDefinition("Active", "Active", BasicType.Bit, PropertyInputType.checkbox);

            this.Properties.Add("Active", active);

            base.SetCustomProperties();
        }
    }

    public class N2M : BusinessBase
    {
        protected string ownFieldNameN = "";
        protected string externalFieldNameM = "";
        protected string ownFieldNameM = "";

        protected string[] opcCampoOtros;

        public N2M(string tableName) : base(tableName)
        {

        }

        public override string Key
        {
            get
            {
                //if (this[ownFieldNameM] is DBNull)
                //{
                //    return fila.ClaveNuevo;
                //}

                if (this.Parent.Contains(this))
                {
                    return this[externalFieldNameM].ToString();
                }
                else
                {
                    return dataItem.Key;
                }
            }
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
            int codigoM = (int)base[externalFieldNameM] != 0 ? (int)base[externalFieldNameM] : (int)base[ownFieldNameM];
            bool active = (bool)this["Active"];

            if (IsDeleting || !active)
            {
                base[ownFieldNameN] = Parent.Parent.Key.NoNullInt();
                base[ownFieldNameM] = codigoM;

                if (!IsDeleting)
                {
                    IsDeleting = true;
                }

                base.StoreToDB();
            }
            else
            {
                string sql;

                sql = "Select count(*) From " + Definition.TableNameEncapsulated
                    + " WHERE " + ownFieldNameN + " = " + Parent.Parent.Key
                    + " AND " + ownFieldNameM + " = " + codigoM;
                if (DB.Instance.QueryFirstOrDefault<int>(sql) == 0)
                {
                    this.SetNew();
                }

                base[ownFieldNameN] = Parent.Parent.Key.NoNullInt();
                base[ownFieldNameM] = codigoM;

                base.StoreToDB();
            }
        }

        public override bool MatchFilter(string filterName)
        {
            return ((bool)this["Active"]);
        }

        public override object this[string campo]
        {
            get
            {
                if (campo.ToLower() == "active")
                {
                    return (int)base[campo] == 1;
                }
                else
                {
                    return base[campo];
                }
            }
            set
            {
                if (campo.ToLower() == "active")
                {
                    base[campo] = (bool)value ? 1 : 0;
                }
                else
                {
                    base[campo] = value;
                }
            }
        }
    }
}
