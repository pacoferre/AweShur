using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AweShur.Core.Security;

namespace AweShur.Web.Demo.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class HomeController : Controller
    {
        private IMemoryCache cache;

        public HomeController(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Exit()
        {
            Request.HttpContext.Session.Clear();

            return Redirect("/");
        }

        public class LoginObject
        {
            public string username;
            public string password;
        }

        [HttpPost]
        public JsonResult Login([FromBody]LoginObject login)
        {
            AppUser user;

            Request.HttpContext.Session.Clear();

            user = AppUser.Login(login.username, login.password, HttpContext.Session);

            if (user == null)
            {
                HttpContext.Session.Clear();
            }

            return new JsonResult(new
            {
                result = user != null
            });
        }
    }
}
