using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AweShur.Core.Security;

namespace AweShur.Web.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class HomeController : Controller
    {
        public class LoginObject
        {
            public string email;
            public string password;
        }

        [HttpPost]
        public JsonResult Login([FromBody]LoginObject login)
        {
            bool valid;

            HttpContext.Session.Clear();

            valid = AppUser.Login(login.email, login.password, HttpContext);

            if (!valid)
            {
                HttpContext.Session.Clear();
            }

            return new JsonResult(new
            {
                result = valid
            });
        }
    }
}
