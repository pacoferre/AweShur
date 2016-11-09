using AweShur.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AweShur.Core.DataViews;

namespace AweShur.Web.Demo.Models.Customer
{
    public class CustomerFilter : FilterBase
    {
        public CustomerFilter(BusinessBaseDecorator decorator) : base(decorator, 0)
        {

        }

        public override void SetDataView(DataView dataView)
        {
            base.SetDataView(dataView);

            dataView.FromClause = @"Customer INNER JOIN
CustomerType ON Customer.idCustomerType = CustomerType.idCustomerType";

            dataView.Columns.Insert(1, new DataViewColumn("CustomerType.name", "Type"));
            dataView.Columns.Add(new DataViewColumn(Decorator.TableNameEncapsulated,
                Decorator.Properties["address"]));

            dataView.Columns.Add(new DataViewColumn(Decorator.TableNameEncapsulated,
                Decorator.Properties["nextCall"]));
        }
    }
}
