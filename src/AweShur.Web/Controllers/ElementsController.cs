using AweShur.Core.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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
                if (RouteData.Values["action"].ToString() == "AWSLib")
                {
                    ok = true;
                }
                else
                {
                    object id = RouteData.Values["id"];

                    if (id != null)
                    {
                        ok = (id.ToString() == AppUser.IDAppUser(HttpContext).ToString());
                    }
                }
            }

            if (!ok)
            {
                context.Result = Redirect("/");
            }
        }

        public IActionResult LoadTemplate(string component, int? id)
        {
            return PartialView("~/Views/Elements/Template/" + component + ".cshtml");
        }
        public IActionResult LoadLayout(string component, int? id)
        {
            return PartialView("~/Views/Layouts/" + component + ".cshtml");
        }

        public IActionResult Load(string component, string objectName, int? id)
        {
            return PartialView("~/Views/Elements/" + component + ".cshtml", 
                AweShur.Core.BusinessBaseProvider.Instance.GetDefinition(objectName, 0));
        }

        public IActionResult LoadFolder(string folder, string component, string objectName, int? id)
        {
            return PartialView("~/Views/Elements/" + folder + "/" + component + ".cshtml",
                AweShur.Core.BusinessBaseProvider.Instance.GetDefinition(objectName, 0));
        }

        private static ConcurrentDictionary<string, byte[]> componentCache = new ConcurrentDictionary<string, byte[]>();

//        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 10)]
        public IActionResult AWSLib(string component)
        {
            byte[] content = null;

#if (DEBUG)
            try
            {
                string fileName = component + ".html";
                string directory = GetType().GetTypeInfo().Assembly.Location;

#if (NET46)
                DirectoryInfo info = new DirectoryInfo(directory.Replace("AweShur.Web.dll", "") + @"..\..\..\..\..\AweShur.Web\Components\" + component);
#else
                DirectoryInfo info = new DirectoryInfo(directory.Replace("AweShur.Web.dll", "") + @"..\..\..\..\AweShur.Web\Components\" + component);
#endif

                content = System.IO.File.ReadAllBytes(info.FullName + "\\" + fileName);
            }
            catch
            {

            }
#endif

            if (content == null)
            {
                string name = "AweShur.Web.Components." + component.Replace('-', '_') + "." + component + ".html";

                content = componentCache.GetOrAdd(name, (key) =>
                {
                    using (Stream stream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream(name))
                    {
                        using (MemoryStream temp = new MemoryStream())
                        {
                            stream.CopyTo(temp);

                            return temp.ToArray();
                        }
                    }
                });
            }

            return File(content, "text/html");
        }
    }
}
