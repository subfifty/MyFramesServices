using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http.Headers;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Security.Principal;

//using WebApi.Entities;


namespace XPhoneRestApi
{
#if AUTH_EXPERIMENTAL
    //https://github.com/galenam/MicroserviceTemplate/blob/master/Common/Const/PolicyName.cs

    public static class PolicyName
    {
        public const string PhoneNumber = "NeedPhoneNumber";
    }

    public class PhoneNumberAuthenticationOptions : AuthenticationSchemeOptions
    {
        public Regex PhoneMask { get; set; }// = new Regex("7\\d{10}");

    }

    public class PhoneNumberAuthenticationHandler : AuthenticationHandler<PhoneNumberAuthenticationOptions>
    {
        public PhoneNumberAuthenticationHandler(IOptionsMonitor<PhoneNumberAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var headers = Request.Headers;

            var headerName = "X-PhoneNumber";
            var phones = headers[headerName];
            var phone = phones.ToArray()?.FirstOrDefault();

            if (!string.IsNullOrEmpty(phone) && Options.PhoneMask.IsMatch(phone))
            {
                var claims = new[] { new Claim("phone", phone) };
                var identity = new ClaimsIdentity(claims, "PhoneNumberAuthenticationHandler");
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Unuathorized with phone"));
        }
    }

    public class JWTAccess : AuthorizeAttribute, IAuthorizationFilter
    {
        public JWTAccess()
        {
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip authorization if configured so
            //string Authorization = ConfigurationManager.AppSettings["Authorization"];
            //if (Authorization != null && Authorization.ToLower() == "none")
            //    return true;

            //if (Authorization != null && Authorization.ToLower() != "jwt")
            //    return true;

            ApiConfig.Instance.ReloadConfiguration();
            string Base_API_URL = ApiConfig.Instance.ReadAttributeValue("authorization", "VerifyAccessUrl");
            string AuthMode = ApiConfig.Instance.ReadAttributeValue("authorization", "AuthMode");


            //HTTP Header: Authorization: Bearer <token>
            string token = "";

            var authorization = context.HttpContext.Request.Headers["Authorization"];

            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                // we have a valid AuthenticationHeaderValue that has the following details:
                var scheme = headerValue.Scheme;        // Bearer
                if (scheme.ToLower() == "bearer")
                    token = headerValue.Parameter;      // <token>
            }



            // Beispiel: http://10.1.1.54/XPhoneConnect/ContactApi/Vdir/Verify
            string url = Base_API_URL;

            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            var result = Task.Run(() => client.GetStringAsync(url)).Result;

            //TODO
            // - als JSON parsen
            // - prüfen, ob alles stimmt
            //return this.Content(result, "application/json");


            var isAuthenticated = false; // context.HttpContext.User.Identity.IsAuthenticated;

            try
            {
                IDictionary<string, string> claims = null; // XPhoneAuthenticationService.VerifyAccessToken(httpContext.Request);
                context.HttpContext.Session.SetString("UserGuid", claims["UserGuid"]);
                context.HttpContext.Session.SetString("FullName", claims["FullName"]);
                context.HttpContext.Session.SetString("IdentityToken", claims["IdentityToken"]);
                context.HttpContext.Session.SetString("TokenExpiration", claims["exp"].ToString());
                isAuthenticated = true;
            }
            catch (Exception ex)
            {
                context.HttpContext.Response.Headers.Add("X-Authorize-Details", ex.Message);
            }

            context.HttpContext.Response.Headers.Add("WWW-Authenticate", "Bearer");

            if (!isAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            //var hasAllRequredClaims = _requiredClaims.All(claim => context.HttpContext.User.HasClaim(x => x.Type == claim));
            //if (!hasAllRequredClaims)
            //{
            //    context.Result = new ForbidResult();
            //    return;
            //}
        }
    }
#endif

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

            // General Authorization?
            string AuthMode = ApiConfig.Instance.ReadAttributeValue("authorization", "Authorization");
            if (AuthMode != null && AuthMode.ToLower() != "jwt")
                return;

            // Authorization per Controller?
            AuthMode = ApiConfig.Instance.ReadAttributeValue(m_Controller, "Authorization");
            if (AuthMode != null && AuthMode.ToLower() != "jwt")
                return;

            // URL zur Validierung des Access-Tokens
            string Base_API_URL = ApiConfig.Instance.ReadAttributeValue("authorization", "VerifyAccessUrl");
            string BearerToken = ApiConfig.Instance.ReadAttributeValue("authorization", "BearerToken");

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

            // Sonderlocke für Dashboard-Scripts ohne "echte" Authentifizierung
            if ( token == BearerToken )
            {
                return;
            }

            // Beispiel: http://10.1.1.54/XPhoneConnect/ContactApi/Vdir/Verify
            string url = Base_API_URL;

            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            try
            {
                var result = Task.Run(() => client.GetStringAsync(url)).Result;
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
