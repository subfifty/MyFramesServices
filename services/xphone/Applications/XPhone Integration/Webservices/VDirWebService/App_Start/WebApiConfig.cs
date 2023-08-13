using System.Web.Http;

namespace C4B.VDir.WebService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //config.MapHttpAttributeRoutes();

            //config.EnableCors(new EnableCorsAttribute(
            //    /* Origins */ "*",
            //    /* Headers */ "authorization,refresh-token,clientsecret,accept,content-type",
            //    /* Methods */ "GET"));
            
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
