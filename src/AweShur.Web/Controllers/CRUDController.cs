using AweShur.Core;
using AweShur.Core.REST;
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
                context.Result = new JsonResult(ModelToClient.ErrorResponse("User not authenticated"));
            }
        }

        [HttpGet]
        public IEnumerable<dynamic> List(string objectName)
        {
            return BusinessBaseProvider.RetreiveObject(objectName, "0", HttpContext).Get();
        }

        [HttpPost]
        public JsonResult Get(string objectName, string key)
        {
            return new JsonResult(BusinessBaseProvider.RetreiveObject(objectName, key, HttpContext).ToClient());
        }

        [HttpPost]
        public JsonResult Post([FromBody]BusinessBase obj)
        {
            try
            {
                obj.StoreToDB();

                return new JsonResult(obj.ToClient());
            }
            catch(Exception exp)
            {
                return new JsonResult(ModelToClient.ErrorResponse(exp.Message));
            }
        }

        [HttpPost]
        public void Put(int key, [FromBody]AppUser value)
        {
        }

        [HttpPost]
        public void Delete(int key)
        {
        }
    }
}

