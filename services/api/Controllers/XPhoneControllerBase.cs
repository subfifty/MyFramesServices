using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace XPhoneRestApi.Controllers
{
    /*
    public static class SchemesNamesConst
    {
        public const string TokenAuthenticationDefaultScheme = "TokenAuthenticationScheme";
    }

    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        public IServiceProvider ServiceProvider { get; set; }

        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IServiceProvider serviceProvider)
            : base(options, logger, encoder, clock)
        {
            ServiceProvider = serviceProvider;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var headers = Request.Headers;
            var token = "X-Auth-Token".GetHeaderOrCookieValue(Request);

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Token is null"));
            }

            bool isValidToken = false; // check token here

            if (!isValidToken)
            {
                return Task.FromResult(AuthenticateResult.Fail($"Balancer not authorize token : for token={token}"));
            }

            var claims = new[] { new Claim("token", token) };
            var identity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
    */

    static class ExtensionMethodsForHttpRequest
    {
        public static async Task<string> GetRawBodyAsync(
            this HttpRequest request,
            Encoding encoding = null)
        {
            if (!request.Body.CanSeek)
            {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                request.EnableBuffering();
            }

            request.Body.Position = 0;
            var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            request.Body.Position = 0;

            return body;
        }
    }

    public class XPhoneControllerBase : ControllerBase
    {
        //internal static string GetBody(HttpRequest request)
        //{
        //    var body = request?.Body;
        //    body.Seek(0, System.IO.SeekOrigin.Begin);
        //    using (var reader = new StreamReader(body))
        //    {
        //        return reader.ReadToEnd();
        //    }
        //}

        //internal string GetBody()
        //{
        //    var body = HttpContext.Request.Body;
        //    //body.Seek(0, System.IO.SeekOrigin.Begin);
        //    using (var reader = new StreamReader(body))
        //    {
        //        return reader.ReadToEnd();
        //    }
        //}

        //public static string GetRawBodyString(this HttpContext httpContext, Encoding encoding)
        //{
        //    var body = "";
        //    if (httpContext.Request.ContentLength == null || !(httpContext.Request.ContentLength > 0) ||
        //        !httpContext.Request.Body.CanSeek) return body;
        //    httpContext.Request.EnableRewind();
        //    httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        //    using (var reader = new StreamReader(httpContext.Request.Body, encoding, true, 1024, true))
        //    {
        //        body = reader.ReadToEnd();
        //    }
        //    httpContext.Request.Body.Position = 0;
        //    return body;
        //}

        internal IPAddress GetRemoteIPAddress(bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                // if you are allowing these forward headers, please ensure you are restricting context.Connection.RemoteIpAddress
                // to cloud flare ips: https://www.cloudflare.com/ips/
                try
                {
                    string header = (HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault());
                    if (IPAddress.TryParse(header, out IPAddress ip))
                    {
                        return ip;
                    }
                }
                catch { }
            }
            try
            {
                return HttpContext.Connection.RemoteIpAddress;
            }
            catch
            {
                return IPAddress.Parse("0.0.0.0");
            }
        }

        internal ContentResult Relay_ApiEndpoint_GET(string query = null)
        {
            ApiConfig.Instance.ReloadConfiguration();
            string endpoint = ApiConfig.Instance.ReadAttributeValue("dmz", "ApiEndpoint");

            if ( String.IsNullOrEmpty(query) ) 
            {
                query = this.HttpContext.Request.Path + this.HttpContext.Request.QueryString;
            }

            return RelayHttp_GET(query, endpoint);
        }

        internal ContentResult RelayHttp_GET(string query, string endpoint)
        {
            if (String.IsNullOrEmpty(endpoint))
            {
                return this.Content("Cannot execute query. Missing Api Parameters.", "application/json");
            }

            try
            {
                // Beispiel: https://c4b.zendesk.com/api/v2/users/search?query=simon.gallego@telekom.de
                string url = endpoint + query;
                //url = System.Web.HttpUtility.UrlEncode(url);
                string result = "ERROR";
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 10);
                    client.DefaultRequestHeaders.Add("Accept", "*/*");
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAuthToken());

                    var t = Task.Run(() => client.GetStringAsync(url));
                    t.Wait();
                    result = t.Result;

                    return this.Content(result, "application/json");
                }
            }
            catch (Exception ex)
            {
                return this.Content(ex.ToString(), "application/json");
            }
        }

        internal async Task<ContentResult> RelayHttp_POST(string query, string endpoint, string body = null)
        {
            if (String.IsNullOrEmpty(endpoint))
            {
                return this.Content("Cannot execute query. Missing Api Parameters.", "application/json");
            }

            try
            {
                if (body == null)
                {
                    body = await Request.GetRawBodyAsync();
                }
                var content = new StringContent(body);
                
                if ( !String.IsNullOrEmpty(body))
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string url = endpoint + query;

                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 10);
                    client.DefaultRequestHeaders.Add("Accept", "*/*");
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAuthToken());

                    var t = Task.Run(() => client.PostAsync(url, content));
                    t.Wait();
                    var result = t.Result.Content.ReadAsStringAsync().Result;

                    this.HttpContext.Response.StatusCode = (int)t.Result.StatusCode;
                    return this.Content(result, "application/json");
                }
            }
            catch (Exception ex)
            {
                return this.Content(ex.ToString(), "application/json");
            }
        }

        private string GetAuthToken()
        {
            //HTTP Header: Authorization: Bearer <token>
            string token = "NONE";

            var authorization = ControllerContext.HttpContext.Request.Headers["Authorization"];
            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                // we have a valid AuthenticationHeaderValue that has the following details:
                var scheme = headerValue.Scheme;        // Bearer
                if (scheme.ToLower() == "bearer")
                    token = headerValue.Parameter;      // <token>
            }

            return token;
        }
    }
}
