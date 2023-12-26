using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using C4B.Atlas.Connection;
using System.IO;

using Newtonsoft.Json;
using System.Management.Automation;
using System.Collections.ObjectModel;

using XPhoneRestApi;
using System.Net.Http;
using System.Threading.Tasks;

namespace C4B.VDir.WebService.Controllers
{
    internal class PowerShellHelper
    {
        private static string PowershellControlDirectory
        {
            get
            {
                string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"C4B\Powershell\PowershellControl.Ini");
                string dir = Path.GetDirectoryName(path);
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        /// <summary>
        /// Runs a PowerShell script with parameters and prints the resulting pipeline objects to the console output. 
        /// </summary>
        /// <param name="scriptContents">The script file contents.</param>
        /// <param name="scriptParameters">A dictionary of parameter names and parameter values.</param>
        private static string RunScript(string scriptContents, Dictionary<string, object> scriptParameters)
        {
            // create a new hosted PowerShell instance using the default runspace.
            // wrap in a using statement to ensure resources are cleaned up.

            string result = "ERROR";
            try
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    // specify the script code to run.
                    ps.AddScript(scriptContents);

                    if (scriptParameters != null)
                    {
                        // specify the parameters to pass into the script.
                        ps.AddParameters(scriptParameters);
                    }

                    result = "";

                    // execute the script and await the result.
                    Collection<PSObject> results = ps.Invoke();
                    foreach (PSObject x in results)
                    {
                        PSMemberInfoCollection<PSMemberInfo> memberInfos = x.Members;
                        Console.WriteLine(memberInfos["Id"].Value);
                        result += (memberInfos["Id"].Value.ToString());
                    }

                    // execute the script and await the result.
                    /*
                    var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);

                    // collect the resulting pipeline objects and return 
                    foreach (var item in pipelineObjects)
                    {
                        result += item.BaseObject.ToString();
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        private static string XPhoneDomain = "";
        public static string GetXPhoneDomain()
        {
            if ( string.IsNullOrEmpty(XPhoneDomain))
            {
                string path = Path.Combine(PowershellControlDirectory, "Get-XPhoneDomain.ps1");
                string psScript = System.IO.File.ReadAllText(path);
                string result = RunScript(psScript, null);
                XPhoneDomain = result;
            }

            return XPhoneDomain;
        }

    }

    public class AuthController : Controller
    {
        private string GetBody(HttpRequestBase request)
        {
            var requestStream = request?.InputStream;
            requestStream.Seek(0, System.IO.SeekOrigin.Begin);
            using (var reader = new StreamReader(requestStream))
            {
                return reader.ReadToEnd();
            }
        }

        private JsonResult LogonResult(string UserName, string Password, string ClientSecret)
        {
            string Hostname = ConfigurationManager.AppSettings["WcfIPEndPoint"];

            string errMsg = null;
            AppUser usr = null;
            ATClientConnector client = null;
            try
            {
                usr = XPhoneAuthenticationService.TryGetXPhoneUser(Hostname, UserName, Password, ClientSecret, out errMsg, out client);

                if (usr == null)
                {
                    usr = new AppUser() { Error = "AUTHENTICATION FAILED (usr=null): " + errMsg };
                }

                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                }
            }
            catch (Exception ex)
            {
                usr = new AppUser() { Error = "AUTHENTICATION FAILED (exception): " + ex.Message };
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                }
            }

            return Json(usr, JsonRequestBehavior.AllowGet);
        }

        private JsonResult VerifyResult()
        {
            IDictionary<string, object> claims = new Dictionary<string, object>();

            claims.Add("UserGuid", this.HttpContext.Session["UserGuid"] as string);
            claims.Add("FullName", this.HttpContext.Session["FullName"] as string);
            claims.Add("IdentityToken", this.HttpContext.Session["IdentityToken"] as string);
            claims.Add("TokenExpiration", this.HttpContext.Session["TokenExpiration"] as string);

            return Json(claims, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("Logon")]
        public ActionResult LogonPost()
        {
            string UserName = "";
            string Password = "";
            string ClientSecret = "";

            string body = GetBody(Request);
            MyData param = JsonConvert.DeserializeObject<MyData>(body);

            UserName = param.UserName;
            Password = param.Password;
            ClientSecret = param.ClientSecret;

            return LogonResult(UserName, Password, ClientSecret);
        }

        [HttpPost]
        [ActionName("Refresh")]
        public ActionResult RefreshPost()
        {
            IDictionary<string, string> result = new Dictionary<string, string>();

            string UserGuid = "";
            string FullName = "";
            string ClientSecret = "";
            string AccessToken = "";
            string RefreshToken = "";
            string IdentityToken = "";
            string UserName = "";

            try
            {
                IDictionary<string, object> claims = XPhoneAuthenticationService.VerifyRefreshToken(Request);
                UserGuid = claims["UserGuid"] as string;
                ClientSecret = claims["ClientSecret"] as string;
                FullName = claims["FullName"] as string;
                IdentityToken = claims["IdentityToken"] as string;
                UserName = claims["UserName"] as string;

                AccessToken = XPhoneAuthenticationService.CreateAccessToken(UserGuid, FullName, IdentityToken, UserName);
                RefreshToken = XPhoneAuthenticationService.CreateRefreshToken(UserGuid, ClientSecret, FullName, IdentityToken, UserName);
            }
            catch (Exception ex)
            {
                result["UserGuid"] = null;
                result["ClientSecret"] = null;
                result["AccessToken"] = null;
                result["RefreshToken"] = null;
                result["IdentityToken"] = null;
                result["Error"] = "Failed to refresh tokens: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            result["UserGuid"] = UserGuid;
            result["ClientSecret"] = ClientSecret;
            result["AccessToken"] = AccessToken;
            result["RefreshToken"] = RefreshToken;
            result["IdentityToken"] = IdentityToken;
            result["Error"] = "";
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [ActionName("Verify")]
        [JWTAccess]
        public ActionResult VerifyPost()
        {
            return VerifyResult();
        }

        [HttpGet]
        public ActionResult Logon()
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //{
            //    return Execute_DMZ_GET(this.HttpContext.Request.RawUrl);
            //}

            string UserName = "";
            if (Request.Unvalidated.QueryString["UserName"] != null)
            {
                UserName = Request.Unvalidated.QueryString["UserName"];
            }

            string Password = "";
            if (Request.Unvalidated.QueryString["Password"] != null)
            {
                Password = Request.Unvalidated.QueryString["Password"];
            }

            string ClientSecret = "";
            if (Request.Unvalidated.QueryString["ClientSecret"] != null)
            {
                ClientSecret = Request.Unvalidated.QueryString["ClientSecret"];
            }

            return LogonResult(UserName, Password, ClientSecret);
        }

        //[JWTRefresh]
        [HttpGet]
        public ActionResult Refresh()
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //{
            //    return Execute_DMZ_GET(this.HttpContext.Request.RawUrl);
            //}

            IDictionary<string, string> result = new Dictionary<string, string>();

            string UserGuid = "";
            string FullName = "";
            string ClientSecret = "";
            string AccessToken = "";
            string RefreshToken = "";
            string IdentityToken = "";
            string UserName = "";
            try
            {
                IDictionary<string, object> claims = XPhoneAuthenticationService.VerifyRefreshToken(Request);
                UserGuid = claims["UserGuid"] as string;
                ClientSecret = claims["ClientSecret"] as string;
                FullName = claims["FullName"] as string;
                IdentityToken = claims["IdentityToken"] as string;
                UserName = claims["UserName"] as string;

                AccessToken = XPhoneAuthenticationService.CreateAccessToken(UserGuid, FullName, IdentityToken, UserName);
                RefreshToken = XPhoneAuthenticationService.CreateRefreshToken(UserGuid, ClientSecret, FullName, IdentityToken, UserName);
            }
            catch (Exception ex)
            {
                result["UserGuid"] = null;
                result["ClientSecret"] = null;
                result["AccessToken"] = null;
                result["RefreshToken"] = null;
                result["IdentityToken"] = null;
                result["Error"] = "Failed to refresh tokens: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            result["UserGuid"] = UserGuid;
            result["ClientSecret"] = ClientSecret;
            result["AccessToken"] = AccessToken;
            result["RefreshToken"] = RefreshToken;
            result["IdentityToken"] = IdentityToken;
            result["Error"] = "";
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        [JWTAccess]
        public ActionResult Verify()
        {
            return VerifyResult();
        }

        [HttpGet]
        [ActionName("Help")]
        public string Get()
        {
            if (ApiConfig.Instance.RunningInDMZ())
                return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            return ShowHelp();
        }

        private string ShowHelp()
        {
            string info = "XPhone Connect Authentication API" + "\r\n" + "\r\n";

            string help =
                  @"GET /auth" + "\r\n"
                + @"    Show help." + "\r\n"
                + @"GET /auth/license" + "\r\n"
                + @"    Show license info." + "\r\n"
                ;

            //if (!IsValidLicense())
            //{
            //    help += "\r\n\r\n" + "INVALID LICENSE FOUND!" + "\r\n\r\n";
            //}

            return info + help;
        }

        #region Private
        MvcApplication _application
        {
            get { return HttpContext.ApplicationInstance as MvcApplication; }
        }
        #endregion

    }
}
