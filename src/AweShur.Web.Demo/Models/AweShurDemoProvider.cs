using AweShur.Core;
using AweShur.Web.Demo.Models.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Web.Demo.Models
{
    public class AweShurDemoProvider : BusinessBaseProvider
    {
        public override void RegisterBusinessCreators()
        {
            base.RegisterBusinessCreators();

            creators.Add("Customer", () => new Customer.Customer());
            decorators.Add("Customer", () => new CustomerDecorator());
        }
    }
}
