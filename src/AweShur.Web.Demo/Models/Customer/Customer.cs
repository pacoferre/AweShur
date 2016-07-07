using AweShur.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Web.Demo.Models.Customer
{
    public class CustomerDecorator : BusinessBaseDefinition
    {
        protected override void SetCustomProperties()
        {
            PropertyDefinition prop = Properties["idCustomerType"];

            prop.Type = PropertyInputType.select; // Will not be usefull untill we got HtmlHelpers
            prop.DefaultValue = BusinessBaseProvider.ListProvider.GetList("CustomerType").First[0];
            prop.ListSource = "CustomerType";
            prop.Label = "Customer type";

            Properties["memo"].Type = PropertyInputType.textarea;
        }
    }

    public class Customer : BusinessBase
    {
        public Customer() : base("Customer")
        {
        }

        public override string Description
        {
            get
            {
                int idCustomerType = this["idCustomerType"].NoNullInt();

                if (idCustomerType > 0)
                {
                    return this["name"] + " (" + BusinessBaseProvider.ListProvider.GetList("CustomerType").GetValue(idCustomerType) + ")";
                }

                return this["name"].ToString();
            }
        }
    }
}
