using AweShur.Core;
using AweShur.Web.Demo.Models.Customer;
using AweShur.Web.Demo.Models.Project;
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
            creators.Add("ProjectTask", () => new ProjectTask());

            decorators.Add("Customer", () => new CustomerDecorator());
            decorators.Add("ProjectTask", () => new ProjectTaskDecorator());
        }
    }
}
