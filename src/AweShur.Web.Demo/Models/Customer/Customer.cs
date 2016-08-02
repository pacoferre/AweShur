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
            prop.ListObjectName = "CustomerType";
            prop.Label = "Customer type";

            prop = Properties["idCustomerRanking"];

            prop.Type = PropertyInputType.select; // Will not be usefull untill we got HtmlHelpers
            prop.DefaultValue = BusinessBaseProvider.ListProvider.GetList("CustomerType").First[0];
            prop.ListObjectName = "CustomerRanking";
            prop.Label = "Customer ranking";

            Properties["memo"].Type = PropertyInputType.textarea;
        }
    }

    public class Customer : BusinessBase
    {
        public Customer() : base("Customer")
        {
            this.relatedCollections.Add("ColContact", new BusinessCollectionBase(this, "idCustomer", "Contact",
                "Select * From Contact Where idCustomer = @id Order By name"));
        }

        public override bool Validate()
        {
            bool valid = base.Validate();

            if (valid)
            {
                if (this["name"].NoNullString().ToLower().Contains("utrilla"))
                {
                    this.LastErrorMessage = "La empresa no puede ser Utrilla";
                    this.LastErrorProperty = "name";

                    valid = false;
                }
            }

            return valid;
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
