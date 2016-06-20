using AweShur.Core.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Web.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class ElementsController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool ok = false;

            base.OnActionExecuting(context);

            if (AppUser.UserIsAuthenticated(context.HttpContext))
            {
                object id = RouteData.Values["id"];

                if (id != null)
                {
                    ok = (id.ToString() == AppUser.IDAppUser(HttpContext).ToString());
                }
            }

            if (!ok)
            {
                context.Result = Redirect("/");
            }
        }

        public IActionResult Load(string component, int? id)
        {
            return PartialView("~/Views/Elements/" + component + ".cshtml");
        }
        public IActionResult LoadFolder(string folder, string component, int? id)
        {
            return PartialView("~/Views/Elements/" + folder + "/" + component + ".cshtml");
        }
    }
}
