using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AweShur.Web
{
    public class Startup
    {
        public static void AddRoutes(IRouteBuilder routes)
        {
            routes.MapRoute(
                name: "crudList",
                template: "CRUD/List/{objectName}",
                defaults: new { controller = "CRUD", action = "List" });
            routes.MapRoute(
                name: "crudPost",
                template: "CRUD/Post",
                defaults: new { controller = "CRUD", action = "Post" });
            routes.MapRoute(
                name: "crudGetList",
                template: "CRUD/GetList",
                defaults: new { controller = "CRUD", action = "GetList" });
            routes.MapRoute(
                name: "elementtemplate",
                template: "Elements/LoadTemplate/{component}/{id}",
                defaults: new { controller = "Elements", action = "LoadTemplate" });
            routes.MapRoute(
                name: "elementlayout",
                template: "Elements/LoadLayout/{component}/{id}",
                defaults: new { controller = "Elements", action = "LoadLayout" });
            routes.MapRoute(
                name: "elements1",
                template: "Elements/Load/{folder}/{component}/{objectName}/{id}",
                defaults: new { controller = "Elements", action = "LoadFolder" });
            routes.MapRoute(
                name: "elements",
                template: "Elements/Load/{component}/{objectName}/{id}",
                defaults: new { controller = "Elements", action = "Load" });
            routes.MapRoute(
                name: "AWSLib",
                template: "AWSLib/{component}",
                defaults: new { controller = "Elements", action = "AWSLib" });
        }
    }
}
