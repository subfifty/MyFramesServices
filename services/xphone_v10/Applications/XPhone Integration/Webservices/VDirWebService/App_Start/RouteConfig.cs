using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace C4B.VDir.WebService
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    "Search",
            //    "Search/",
            //    new { controller = "Dashboard", action = "Index" }
            //);

            //routes.MapRoute(
            //    "Search2",
            //    "Search2/",
            //    new { controller = "Dashboard", action = "Index" }
            //);

            //routes.MapRoute(
            //    "Designer",
            //    "Designer/",
            //    new { controller = "Admin", action = "Configuration" }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Applink", id = UrlParameter.Optional }
            );
        }
    }
}