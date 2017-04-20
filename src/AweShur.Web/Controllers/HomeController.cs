using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AweShur.Core.Security;
using AweShur.Web.Helpers;

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
            else
            {
                CookiesHelper.WriteCookie(HttpContext, CookiesHelper.LoginCookieName, login.email, 1);
            }

            return new JsonResult(new
            {
                result = valid
            });
        }

        public class ChangePasswordObject
        {
            public string actual;
            public string newPassword;
        }

        [HttpPost]
        public JsonResult ChangePassword([FromBody]ChangePasswordObject passwordInfo)
        {
            try
            {
                bool valid = AppUser.GetAppUser(HttpContext).ChangePassword(passwordInfo.actual, passwordInfo.newPassword);

                return new JsonResult(new
                {
                    result = valid
                });
            }
            catch(Exception ex)
            {
                return new JsonResult(new
                {
                    result = false,
                    message = ex.Message
                });
            }
        }
    }
}
