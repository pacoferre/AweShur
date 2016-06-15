using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Web.Demo.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class ElementsController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool ok = false;

            base.OnActionExecuting(context);

            if (HomeController.IsAuthenticated(context.HttpContext))
            {
                object id = RouteData.Values["id"];

                if (id != null)
                {
                    int idappuser;

                    if (Int32.TryParse(id.ToString(), out idappuser))
                    {
                        ok = idappuser == HomeController.IDAppUser(HttpContext);
                    }
                }
            }
 
            if (!ok)
            {
                context.Result = Redirect("/");
            }
        }

        // GET: /<controller>/
        public IActionResult Load(string component, int? idappuser)
        {
            return PartialView("~/Elements/" + component);
        }
    }
}
