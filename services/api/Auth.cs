using Microsoft.AspNetCore.Authorization;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;

namespace XPhoneRestApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private string m_Controller;
        public AuthorizeAttribute(string a_Controller) 
        {
            m_Controller = a_Controller;
        } 

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            ApiConfig.Instance.ReloadConfiguration();

            //// Skip authorization in DMZ (because it's done later)
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return;

            // General Authorization?
            string AuthMode = ApiConfig.Instance.ReadAttributeValue("authorization", "Authorization");
            if (AuthMode != null && AuthMode.ToLower() != "jwt")
                return;

            // Authorization per Controller?
            AuthMode = ApiConfig.Instance.ReadAttributeValue(m_Controller, "Authorization");
            if (AuthMode != null && AuthMode.ToLower() != "jwt")
                return;

            // URL zur Validierung des Access-Tokens
            // AuthEndpoint = "http://10.1.1.54/XPhoneConnect/myframes/xphone/auth"
            string AuthEndpoint = ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpoint");

            //HTTP Header: Authorization: Bearer <token>
            string token = "NONE";

            var authorization = context.HttpContext.Request.Headers["Authorization"];
            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                // we have a valid AuthenticationHeaderValue that has the following details:
                var scheme = headerValue.Scheme;        // Bearer
                if (scheme.ToLower() == "bearer")
                    token = headerValue.Parameter;      // <token>
            }

            // Wenn der Request aus einem APPLINK Dashboard kommt, braucht man keine Authentifizierung!
            var referer = context.HttpContext.Request.Headers.Referer.ToString().ToLower();
            if (referer != null) 
            { 
                if ( referer.Contains("/xphoneconnect/applink") )
                {
                    return;
                }
            }

            /*
            // Sonderlocke für Dashboard-Scripts ohne "echte" Authentifizierung
            // ==> braucht es jetzt nicht mehr! Wird über den Referer abgewickelt!
            string BearerToken = ApiConfig.Instance.ReadAttributeValue("authorization", "BearerToken");
            if ( token == BearerToken )
            {
                return;
            }
            */

            string endpoint = AuthEndpoint + "/verify";
            
            if (ApiConfig.Instance.UseWebapi())
            {
                endpoint = ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpointWebApi");
                endpoint += "/configuration/getCollaborationSection";
                //return RelayHttp_GET("/configuration/getCollaborationSection", endpoint);
            }

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 10);
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var t = Task.Run(() => client.GetStringAsync(endpoint));
                    t.Wait();
                    string result = t.Result;
                }
                catch (Exception ex)
                {
                    // not logged in or role not authorized
                    context.Result = new JsonResult(new { message = "Unauthorized: " + ex.Message })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }
            }
        }
    }
}
