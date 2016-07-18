using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AweShur.Web.Demo.Models;
using AweShur.Core;
using AweShur.Web.Demo.Controllers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching;
using StackExchange.Redis;
using AweShur.Web.Demo.Views.Elements;
using static AweShur.Web.Demo.Views.Elements.CRUDLocations;

namespace AweShur.Web.Demo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddSession(o => {
                IConfigurationSection section = Configuration.GetSection("Session");
                int minutes = section["Expiration"].NoNullInt();
                string cookieName = section["CookieName"] ?? ".AWSD";

                if (minutes == 0)
                {
                    minutes = 10;
                }

                o.IdleTimeout = TimeSpan.FromMinutes(minutes);
                o.CookieName = cookieName;

                AweShur.Web.Helpers.CookiesHelper.CookieName = cookieName;
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSession();

            app.UseMvc(routes =>
            {
                AweShur.Web.Startup.AddRoutes(routes);

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");

            BusinessBaseProvider.Configure(
                app.ApplicationServices.GetRequiredService<IHttpContextAccessor>(),
                redis, new AweShurDemoProvider(), Configuration);


            CRUDLocations.Locations = new Dictionary<string, CRUDLocationItem>
            {
                { "Customer", new CRUDLocationItem { Folder = "Customer" } },
                { "User", new CRUDLocationItem { Folder = "Security", ControlName = "AppUser" } },
            };
        }
    }
}
