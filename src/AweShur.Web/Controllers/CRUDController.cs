using AweShur.Core;
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
    public class CRUDController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!AppUser.UserIsAuthenticated(context.HttpContext))
            {
                context.Result = new JsonResult(BusinessBaseResponse.ErrorResponse);
            }
        }

        [HttpGet]
        public IEnumerable<dynamic> List(string objectName)
        {
            return BusinessBaseProvider.RetreiveObject(objectName, "0", HttpContext).Get();
        }

        [HttpPost]
        public JsonResult Get(string objectName, string id)
        {
            return new JsonResult(BusinessBaseProvider.RetreiveObject(objectName, id, HttpContext).ToJson());
        }

        [HttpPost]
        public JsonResult Post([FromBody]BusinessBase obj)
        {
            try
            {
                obj.StoreToDB();

                return new JsonResult(obj.ToJson());
            }
            catch
            { }

            return new JsonResult(BusinessBaseResponse.ErrorResponse);
        }

        [HttpPost]
        public void Put(int id, [FromBody]AppUser value)
        {
        }

        [HttpPost]
        public void Delete(int id)
        {
        }
    }
}

