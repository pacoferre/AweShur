using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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

        //[HttpPost]
        //public JsonResult Login([FromBody]LoginObject login)
        //{
        //    AppUser user;

        //    Request.HttpContext.Session.Clear();

        //    user = AppUser.Login(login.username, login.password);

        //    if (user == null)
        //    {
        //        HttpContext.Session.SetInt32("idappuser", 0);

        //        return new JsonResult(new
        //        {
        //            result = false
        //        });
        //    }

        //    HttpContext.Session.SetInt32("idappuser", user.idappuser);
        //    HttpContext.Session.SetString("name_shurname", user.name + " " + user.surname);

        //    return new JsonResult(new
        //    {
        //        result = true
        //    });
        //}

        //public class RegisterObject
        //{
        //    public string email;
        //    public string pass;
        //    public string name;
        //    public string surname;
        //}

        //[HttpPost]
        //public JsonResult Register([FromBody]RegisterObject newuser)
        //{
        //    AppUser user = new AppUser();

        //    user.email = newuser.email;
        //    user.pass = newuser.pass;
        //    user.name = newuser.name;
        //    user.surname = newuser.surname;

        //    Request.HttpContext.Session.Clear();

        //    ModelResponse response = user.RegisterNew();

        //    if (!response.ok)
        //    {
        //        HttpContext.Session.SetInt32("idappuser", 0);

        //        if (Debugger.IsAttached)
        //            Trace.WriteLine(response.message);

        //        return new JsonResult(response);
        //    }

        //    HttpContext.Session.SetInt32("idappuser", user.idappuser);
        //    HttpContext.Session.SetInt32("idappusertype", user.idappusertype);
        //    HttpContext.Session.SetString("name_shurname", user.name + " " + user.surname);

        //    return new JsonResult(response);
        //}

        public static bool IsAuthenticated(HttpContext req)
        {
            return req.Session.GetInt32("idappuser") > 0;
        }

        public static int? IDAppUser(HttpContext req)
        {
            return req.Session.GetInt32("idappuser");
        }
    }
}
