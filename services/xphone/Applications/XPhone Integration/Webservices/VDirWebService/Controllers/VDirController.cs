using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using C4B.Atlas.Query;
using C4B.Atlas.Serialisation;
using C4B.Atlas.VDir;
using C4B.Atlas.Visualization;
using C4B.Atlas.Visualization.Wcf.Contracts.VDir;
using C4B.GUI.Framework;
using C4B.Atlas.Connection;
using C4B.Atlas.VDir.Extensions;
using C4B.Atlas.VDir.Mapping;

using System.Runtime.InteropServices;
using System.Security;

using C4B.Dashboard;
using JWT.Builder;
using JWT.Algorithms;
using System.Net.Http.Headers;
using System.IO;

using Newtonsoft.Json;
using XPhoneRestApi;
using System.Web.Optimization;
using System.Net;
using System.Collections.Specialized;

namespace C4B.VDir.WebService.Controllers
{
    #region Helper Classes
    internal static class CredentialStoreHelper
    {
        /// <summary>
        /// Liefert aus einem SecureString den lesbaren String zurück
        /// </summary>
        public static String GetStringFromSecureString(SecureString a_protectedString)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(a_protectedString);
            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(bstr);
            }
        }

        /// <summary>
        /// Liefert aus einem SecureString den lesbaren String zurück
        /// </summary>
        public static SecureString CreateSecureStringFromString(String a_stringToProtect)
        {
            if (a_stringToProtect == null)
                a_stringToProtect = string.Empty;

            var result = new SecureString();

            foreach (var c in a_stringToProtect)
                result.AppendChar(c);

            return result;
        }
    }

    internal class MyData
    {
        // Logon
        public string UserName;
        public string Password;
        public string ClientSecret;

        // Verify
        public string RefreshToken;

        // Query
        public string withPhoto;    // true | false | 1 | 0
        public string full;         // full text query
        public string phone;        // phone number
        public string dn;           // distinguished name
    }

    internal class AppUser
    {
        public bool IsAuthenticated { get; set; }
        public bool IsAdmin { get; set; }
        public string SessionGUID { get; set; }
        public string UserGUID { get; set; }
        public string CultureInfoName { get; set; }
        public string FullName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdentityToken { get; set; }
        public string Error { get; set; }
        public string UserName { get; set; }
        public string Tenant { get; set; }
    }

    internal class BaseService : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ---------- Managed Ressourcen freigeben: ----------


            }

            // ---------- Native Ressourcen hier freigeben: ----------
        }
    }

    internal class XPhoneAuthenticationService : BaseService
    {
        // https://www.grc.com/passwords.htm
        public const string AccessTokenSecret  = "CBLPe31qj9hIxcyD3NDD8mTjqGlhpU5YMvgq8ulp";
        public const string RefreshTokenSecret = "6oMzxmEPq5qQSW7ck9TaLchSBhDT1rAXc0DuUcxe";
        public const string DummyClientSecret  = "YouNeverWalkAlone";

        private static string GetBody(HttpRequestBase request)
        {
            var requestStream = request?.InputStream;
            requestStream.Seek(0, System.IO.SeekOrigin.Begin);
            using (var reader = new StreamReader(requestStream))
            {
                return reader.ReadToEnd();
            }
        }

        public static IDictionary<string, object> VerifyRefreshToken(HttpRequestBase request)
        {
            string RefreshToken = "";
            string ClientSecret = "";

            // POST: Body auswerten
            try
            {
                string body = GetBody(request);
                MyData param = JsonConvert.DeserializeObject<MyData>(body);

                ClientSecret = param.ClientSecret;
                RefreshToken = param.RefreshToken;
            }
            catch { }


            // GET: HTTP Header auswerten, wenn im Body kein RefreshToken vorhanden war.
            if ( string.IsNullOrEmpty(RefreshToken))
            {
                try
                {
                    RefreshToken = request.Headers.Get("Refresh-Token");    //HTTP Header: Refresh-Token: <token>
                    ClientSecret = request.Headers.Get("ClientSecret");
                }
                catch { }
            }

            if (string.IsNullOrEmpty(ClientSecret))
            {
                ClientSecret = XPhoneAuthenticationService.DummyClientSecret;
            }

            IDictionary<string, object> claims = JwtBuilder.Create()
                        .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                        .WithSecret(XPhoneAuthenticationService.RefreshTokenSecret)
                        .MustVerifySignature()
                        .Decode<IDictionary<string, object>>(RefreshToken);
            if (claims["ClientSecret"] as string == ClientSecret)
                return claims;

            throw (new Exception("client secret validation failed"));
        }

        public static IDictionary<string, object> VerifyAccessToken(HttpRequestBase request)
        {
            //HTTP Header: Authorization: Bearer <token>
            string token = "";

            var authorization = request.Headers.Get("Authorization");

            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                // we have a valid AuthenticationHeaderValue that has the following details:
                var scheme = headerValue.Scheme;        // Bearer
                if (scheme.ToLower() == "bearer")
                    token = headerValue.Parameter;      // <token>
            }

            return JwtBuilder.Create()
                        .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                        .WithSecret(XPhoneAuthenticationService.AccessTokenSecret)
                        .MustVerifySignature()
                        .Decode<IDictionary<string, object>>(token);
        }

        public static string CreateAccessToken(string UserGuid, string FullName, string IdentityToken, string UserName)
        {
            return JwtBuilder.Create()
                             .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                             .WithSecret(AccessTokenSecret)
                             .AddClaim("exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds())
                             .AddClaim("IdentityToken", IdentityToken)
                             .AddClaim("UserGuid", UserGuid)
                             .AddClaim("FullName", FullName)
                             .AddClaim("UserName", UserName)
                             .Encode();
        }

        public static string CreateRefreshToken(string UserGuid, string ClientSecret, string FullName, string IdentityToken, string UserName)
        {
            return JwtBuilder.Create()
                             .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                             .WithSecret(RefreshTokenSecret)
                             .AddClaim("exp", DateTimeOffset.UtcNow.AddDays(128).ToUnixTimeSeconds())
                             .AddClaim("IdentityToken", IdentityToken)
                             .AddClaim("UserGuid", UserGuid)
                             .AddClaim("ClientSecret", ClientSecret)
                             .AddClaim("FullName", FullName)
                             .AddClaim("UserName", UserName)
                             .Encode();
        }

        public static AppUser TryGetXPhoneUser(string hostname, string userName, string password, string client_secret, out string errorMessage, out ATClientConnector clientConnector)
        {
            errorMessage = String.Empty;
            clientConnector = null;

            /*
            if (!userName.Contains("@"))
            {
                string xpDomain = ApiConfig.Instance.ReadAttributeValue("authorization", "XPhoneDomain", "");
                xpDomain = PowerShellHelper.GetXPhoneDomain();

                if ( !string.IsNullOrEmpty(xpDomain) )
                {
                    userName += "@" + xpDomain;
                }
            }
            */

            if (string.IsNullOrEmpty(client_secret))
                client_secret = DummyClientSecret;

            try
            {
                var connector = ATClientConnector.CreateWithCustomLogin<IXPhoneContract>
                (
                    hostname,
                    new ATClientLoginData(ATClientLoginMode.Atlas)
                    {
                        UcUsername = userName,
                        Password = CredentialStoreHelper.CreateSecureStringFromString(password)
                    }
                );

                var conResult = connector.OpenInstantly();

                if (conResult.State == ATConnectionState.Connected)
                {
                    // Anmeldung erfolgreich AppUser zurückgeben
                    var userInfo = connector.ConnectionStateInfo.UserInfo;

                    ATContractWrapper<IATServiceBaseContract> service;
                    service = ATContractWrapper<IATServiceBaseContract, IATServiceBaseCallbackContract>.Create(connector.Unity, new ATServiceBaseCallbackContractWrapper());
                    string IdentityToken = service.Interface.GetCurrentUserIdentityToken(new TimeSpan(1, 0, 0)).Item1;

                    //string IdentityToken = "dummy";

                    // ATClientConnector zurückgeben
                    clientConnector = connector;

                    return new AppUser()
                    {
                        IsAuthenticated = true,
                        IsAdmin = userInfo.IsAdmin,
                        SessionGUID = Guid.NewGuid().ToString(),
                        UserGUID = userInfo.Guid.ToString(),
                        CultureInfoName = userInfo.Language.Name,
                        FullName = userInfo.Fullname,
                        AccessToken = CreateAccessToken(userInfo.Guid.ToString(), userInfo.Fullname, IdentityToken, userName),
                        RefreshToken = CreateRefreshToken(userInfo.Guid.ToString(), client_secret, userInfo.Fullname, IdentityToken, userName),
                        IdentityToken = IdentityToken,
                        Error = "",
                        UserName = userName,
                        Tenant = ""
                    };
                }

                // Im Fehlerfall ATClientConnector abräumen
                connector.Close();
                connector.Dispose();

                errorMessage = "ResX.Ids_CollabLogin_ServerNotYetStarted";
                return null;
            }
            catch (ATClientConnectorServerUnavailableException)
            {
                errorMessage = "ResX.Ids_CollabLogin_ServerNotYetStarted";
                return null;
            }

            catch
            {
                errorMessage = "ResX.Ids_CollabLogin_AuthenticationFailed";
                return null;
            }

        }
    }

    //public class JWTRefresh : AuthorizeAttribute
    //{
    //    protected override bool AuthorizeCore(HttpContextBase httpContext)
    //    {
    //        // Skip authorization if configured so
    //        string Authorization = ConfigurationManager.AppSettings["Authorization"];
    //        if (Authorization != null && Authorization.ToLower() == "none")
    //            return true;

    //        if (Authorization != null && Authorization.ToLower() != "jwt")
    //            return true;

    //        try
    //        {
    //            IDictionary<string, object> claims = XPhoneAuthenticationService.VerifyRefreshToken(httpContext.Request);

    //            httpContext.Session["UserGuid"] = claims["UserGuid"];
    //            httpContext.Session["ClientSecret"] = claims["ClientSecret"];
    //            httpContext.Session["FullName"] = claims["FullName"];
    //            httpContext.Session["IdentityToken"] = claims["IdentityToken"];
    //            httpContext.Session["TokenExpiration"] = claims["exp"].ToString();
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            httpContext.Response.Headers.Add("X-Authorize-Details", ex.Message);
    //        }

    //        httpContext.Response.Headers.Add("WWW-Authenticate", "Bearer");
    //        return false;
    //    }
    //}

    public class JWTAccess : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Skip authorization if configured so
            string Authorization = ConfigurationManager.AppSettings["Authorization"];
            if (Authorization != null && Authorization.ToLower() == "none")
                return true;

            if (Authorization != null && Authorization.ToLower() != "jwt")
                return true;

            try
            {
                IDictionary<string, object> claims = XPhoneAuthenticationService.VerifyAccessToken(httpContext.Request);
                httpContext.Session["UserGuid"] = claims["UserGuid"];
                httpContext.Session["FullName"] = claims["FullName"];
                httpContext.Session["IdentityToken"] = claims["IdentityToken"];
                httpContext.Session["TokenExpiration"] = claims["exp"].ToString();
                return true;
            }
            catch (Exception ex)
            {
                httpContext.Response.Headers.Add("X-Authorize-Details", ex.Message);
            }

            httpContext.Response.Headers.Add("WWW-Authenticate", "Bearer");
            return false;
        }
    }

    #endregion

    public class VDirController : Controller
    {
        private static string ControllerName = "vdir";

        internal IPAddress GetRemoteIPAddress(bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                // if you are allowing these forward headers, please ensure you are restricting context.Connection.RemoteIpAddress
                // to cloud flare ips: https://www.cloudflare.com/ips/
                try
                {
                    string header = HttpContext.Request.Headers["CF-Connecting-IP"].ToString();
                    if (IPAddress.TryParse(header, out IPAddress ip))
                    {
                        return ip;
                    }
                }
                catch { }
                try
                {
                    string header = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
                    if (IPAddress.TryParse(header, out IPAddress ip))
                    {
                        return ip;
                    }
                }
                catch { }
            }
            try
            {
                return IPAddress.Parse(HttpContext.Request.UserHostAddress);
            }
            catch
            {
                return IPAddress.Parse("0.0.0.0");
            }
        }

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
        [ActionName("Query")]
        [JWTAccess]
        public ActionResult QueryPost()
        {
            string body = GetBody(Request);
            MyData param = JsonConvert.DeserializeObject<MyData>(body);

            string withPhotoParam = param.withPhoto;
            string full = param.full;
            string phone = param.phone;
            string dn = param.dn;

            Contact contact = new Contact();

            bool withPhoto = true;
            if (withPhotoParam == "0" || withPhotoParam.ToLower() == "false")
                withPhoto = false;

            if (!string.IsNullOrEmpty(phone))
            {
                contact.WorkPhone = phone;
            }
            if (!string.IsNullOrEmpty(full))
            {
                contact.DisplayName = full;
            }
            if (!string.IsNullOrEmpty(dn))
            {
                contact.ID = dn;
                withPhoto = true;
            }

            List<Contact> contactParams = new List<Contact>();
            contactParams.Add(contact);

            return Json(GetContacts(contactParams, Request, withPhoto), JsonRequestBehavior.AllowGet);
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
        public ActionResult Query()
        {
            // claims["exp"]
            // claims["UserGuid"]

            Contact contact = new Contact();
            string query = "xxx";

            bool withPhoto = true;
            if (Request.Unvalidated.QueryString["withPhoto"] != null)
            {
                string value = Request.Unvalidated.QueryString["withPhoto"].ToLower();
                if (value == "0" || value == "false")
                    withPhoto = false;
            }

            string displayName = "";
            if (Request.Unvalidated.QueryString["DisplayName"] != null)
            {
                displayName = Request.Unvalidated.QueryString["DisplayName"];
                contact.DisplayName = displayName;
            }

            string phone = "";
            if (Request.Unvalidated.QueryString["phone"] != null)
            {
                phone = Request.Unvalidated.QueryString["phone"];
                if ( phone.StartsWith(" ") )
                    phone = phone.Replace(" ", "00");
                contact.WorkPhone = phone;
                query = phone;
            }

            string full = "";
            if (Request.Unvalidated.QueryString["full"] != null)
            {
                full = Request.Unvalidated.QueryString["full"];
                contact.DisplayName = full;
            }

            string id = "";
            if (Request.Unvalidated.QueryString["dn"] != null)
            {
                id = Request.Unvalidated.QueryString["dn"];
                contact.ID = id;
                withPhoto = true;
            }
            else if (Request.Unvalidated.QueryString["id"] != null)
            {
                id = Request.Unvalidated.QueryString["id"];
                contact.ID = id;
                withPhoto = true;
            }

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' Query('{1}')", client, query), true);


            List<Contact> contactParams = new List<Contact>();
            contactParams.Add(contact);

            //return Json(GetContacts(contactParams, Request, withPhoto), "application/json", System.Text.Encoding.UTF8, JsonRequestBehavior.AllowGet);
            return Json(GetContacts(contactParams, Request, withPhoto), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [JWTAccess]
        public ActionResult Verify()
        {
            return VerifyResult();
        }
        
        [HttpGet]
        public string Get()
        {
            return ShowHelp();
        }

        private string ShowHelp()
        {
            string info = "XPhone Connect Contact API" + "\r\n" + "\r\n";

            string help =
                  @"GET /contactapi" + "\r\n"
                + @"    Show help." + "\r\n"
                + @"GET /contactapi/license" + "\r\n"
                + @"    Show license info." + "\r\n"
                ;

            //if (!IsValidLicense())
            //{
            //    help += "\r\n\r\n" + "INVALID LICENSE FOUND!" + "\r\n\r\n";
            //}

            return info + help;
        }


        #region Private
        private List<VDirContact> GetContacts(List<Contact> contactParams, HttpRequestBase request = null, bool withPhoto = false)
        {
            var contacts = new List<VDirContact>();

            _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, String.Format("Start vDir search with {0} CONTACTS from request parameters", contactParams.Count));

            List<VDRecord> searchResults = Search(contactParams, request);

            TraceExtension.Info("Search completed");

            if (searchResults != null)
            {
                Dictionary<string, string> items = new Dictionary<string, string>();
                foreach (VDRecord record in searchResults.Where(r => 0 == string.Compare(r[VDirCltDefines.AppLinkColumns.FuzzyMatch].GetFirstStringValue(), "False", StringComparison.InvariantCultureIgnoreCase )))
                {
                    VDirContact contact = new VDirContact(record, withPhoto);
                    if (contacts.Where(c => c.ID == contact.ID).Count() == 0)
                    {
                        contacts.Add(contact);
                    }
                }
            }
            else
            {
                _application?.CurrentDebugInfo?.Failed(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, "No matches retrieved from VDir");
            }
            return contacts;
        }

        private List<VDRecord> Search(List<Contact> contacts, HttpRequestBase request = null)
        {
            List<VDRecord> cummulatedResults = new List<VDRecord>();
            foreach (Contact contact in contacts)
            {
                VDRecord[] result = null;
                if (!String.IsNullOrEmpty(contact.ID))
                {
                    var requestForDebug = "vDir CONTACT search by ID: " + contact.ID;
                    _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                    result = Search(new ATQueryElementEqual("ID", contact.ID), requestForDebug, request);
                }
                else if (ConfigurationManager.AppSettings["SEARCH-BY-EMAIL-PHONE"] == "1")
                {
                    if (!String.IsNullOrEmpty(contact.PrimaryEmailAddress))
                    {
                        var requestForDebug = "vDir CONTACT search by EMAIL1: " + contact.PrimaryEmailAddress;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementEqual("EMAIL1", contact.PrimaryEmailAddress), requestForDebug, request);
                    }
                    else if (!String.IsNullOrEmpty(contact.WorkPhone))
                    {
                        var requestForDebug = "vDir CONTACT search by WorkPhone: " + contact.WorkPhone;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementPhoneNumber(null, contact.WorkPhone), requestForDebug, request);
                    }
                    else if (!String.IsNullOrEmpty(contact.MobilePhone))
                    {
                        var requestForDebug = "vDir CONTACT search by MobilePhone: " + contact.MobilePhone;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementPhoneNumber(null, contact.MobilePhone), requestForDebug, request);
                    }
                    else if (!String.IsNullOrEmpty(contact.HomePhone))
                    {
                        var requestForDebug = "vDir CONTACT search by HomePhone: " + contact.HomePhone;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementPhoneNumber(null, contact.HomePhone), requestForDebug, request);
                    }
                    else if (!String.IsNullOrEmpty(contact.DisplayName))
                    {
                        var requestForDebug = "vDir CONTACT search by DisplayName: " + contact.DisplayName;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementFullTextSearch(null, contact.DisplayName), requestForDebug, request);
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(contact.DisplayName))
                    {
                        var requestForDebug = "vDir CONTACT search by DisplayName: " + contact.DisplayName;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementFullTextSearch(null, contact.DisplayName), requestForDebug, request);
                    }
                    else if (!String.IsNullOrEmpty(contact.PrimaryEmailAddress))
                    {
                        var requestForDebug = "vDir CONTACT search by PrimaryEmailAddress: " + contact.PrimaryEmailAddress;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementEqual("EMAIL1", contact.PrimaryEmailAddress), requestForDebug, request);
                    }
                    else if (!String.IsNullOrEmpty(contact.WorkPhone))
                    {
                        var requestForDebug = "vDir CONTACT search by WorkPhone: " + contact.WorkPhone;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementPhoneNumber(null, contact.WorkPhone), requestForDebug, request);
                    }
                    else if (!String.IsNullOrEmpty(contact.MobilePhone))
                    {
                        var requestForDebug = "vDir CONTACT search by MobilePhone: " + contact.MobilePhone;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementPhoneNumber(null, contact.MobilePhone), requestForDebug, request);
                    }
                    else if (!String.IsNullOrEmpty(contact.HomePhone))
                    {
                        var requestForDebug = "vDir search by HomePhone: " + contact.HomePhone;
                        _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, requestForDebug);
                        result = Search(new ATQueryElementPhoneNumber(null, contact.HomePhone), requestForDebug, request);
                    }
                }

                if (result != null)
                {
                    cummulatedResults.AddRange(result);
                }
            }
            return cummulatedResults;
        }

        private VDRecord[] Search(IATQueryElement atQueryElement, string requestForDebug, HttpRequestBase request = null, String adapter = null, bool withPhoto = false)
        {
            try
            {
                TraceExtension.Info("VDir Search START");

                if (IsWcfConnectionStateInvalid())
                {
                    TraceExtension.Warn(string.Format("WCF Connection State is invalid ('{0}'). DisposeSession to force reinitialization.", 
                        FRWClientConnector.Instance.ClientConnector.ConnectionStateInfo.ConnectionState));
                    FRWClientConnector.Instance.DisposeSession();
                }

                IVDResourceExplorerContract vdResourceExplorer
                    = FRWClientConnector.Instance.Interface<IVDResourceExplorerContract>();

                IATVisualFilter filter = vdResourceExplorer.GetFilterObject("VDir.VDirSearch");
                filter.RemoveAllFilterElements();

                IATVisualFilterElement filterElementEndPointName = filter.AddFilterElement("EndpointName");
                string endpointname = ConfigurationManager.AppSettings["EndpointName"];
                if(String.IsNullOrEmpty(endpointname))
                    endpointname = "AppLink";

                filterElementEndPointName.FilterValue = endpointname;

                IATVisualFilterElement filterElementATQuery = filter.AddFilterElement("ATQuery");
                ATQuery query = new ATQuery();
                if (adapter != null)
                    query.SetBaseDN2Adapter(adapter);
                else
                    query.SetBaseDN2Root();
                query.SearchModifiers = new string[] { "BestPrio" };

                if (request == null)
                {
                    request = HttpContext.Request;
                }

                // UserGuid aus dem JWT Token in die Suche übertragen
                if (HttpContext.Session["UserGuid"] != null)
                {
                    query.UserGuid = new Atlas.ATGuid(HttpContext.Session["UserGuid"] as string);
                }

                // Lois Lane /Debug)
                //query.UserGuid = new Atlas.ATGuid("{bb661ee6-efdc-4199-bb4b-96b9d49cef97}");

                //// Nur beim Aufruf über die WebApplication "AppLink2" ist gewährt, dass 
                //// die WindowsUserSid in der LogonUserIdentity vorhanden ist.
                //// Die WebApplication wird nur mit integrierter WindowsAuthentication eingerichtet.
                //if (null != request.Url &&
                //    !request.Url.AbsoluteUri.ToLower().Contains("applink/") &&
                //    null != request.LogonUserIdentity && null != request.LogonUserIdentity.User)
                //{
                //    string sid = request.LogonUserIdentity.User.ToString();
                //    try
                //    {
                //        sid = ATSID.Create(sid).Name;
                //    }
                //    catch (Exception ex)
                //    {
                //        TraceExtension.Error("Can't get username from sid: " + ex.Message);
                //    }
                //    _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, String.Format("User: \"{0}\", integrated windows authentication", sid));
                //    query.UserSID = request.LogonUserIdentity.User.ToString();
                //}
                //else
                //{
                //    _application?.CurrentDebugInfo?.Unknown(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, "User: Anonymous");
                //}

                //string identityToken = null;

                //if ((identityToken = HttpContext.Session["IdentityToken"] as string ) != null )
                //{
                //    IATVisualFilterElement filterElementIdentityToken
                //        = filter.AddFilterElement("IdentityToken");

                //    filterElementIdentityToken.FilterValue
                //        = identityToken;
                //}

                query.Filter = atQueryElement;
                StreamingContext ctx = new StreamingContext();
                string atQuerySerial = ATSerialisationServices.ToBinaryString(query, ctx);
                filterElementATQuery.FilterValue = atQuerySerial;

                Log.Common.INFO("WCF VDir execution - START");

                ATVisualTransportContainer resourceContainer
                    = (ATVisualTransportContainer)vdResourceExplorer.GetContextList(
                        VDirCltDefines.Visualization.ObjectGuidVDirSearch, 
                        VDirCltDefines.Visualization.ObjectTypeVDirSearch, 
                        false, 
                        ATVisualResourceTypes.Resources, 
                        false, 
                        filter);

                Log.Common.INFO("WCF VDir execution - FINISHED");

                if (null == resourceContainer)
                {
                    LogErrorAndAddToDebugInfoForClient("vdResourceExplorer.GetContextList returned null", requestForDebug);
                    return null;
                }

                VDRecord[] searchResults = null;

                if (resourceContainer.ResultCode == ATVisualServices.EnResourceContainerResultCodes.NO_LICENSE)
                {
                    LogErrorAndAddToDebugInfoForClient("NoLicense ", requestForDebug);
                    throw new UnauthorizedAccessException();
                }

                if (resourceContainer.ResultCode == ATVisualServices.EnResourceContainerResultCodes.VDIR_ENDPOINT_MISSING)
                {
                    LogErrorAndAddToDebugInfoForClient("Enpdoint is missing", requestForDebug);
                    return null;
                }

                if (null == resourceContainer.ObjectData)
                {
                    LogErrorAndAddToDebugInfoForClient("vdResourceExplorer returned empty ObjectData", requestForDebug);
                    return null;
                }

                if (null == (searchResults = resourceContainer.ObjectData as VDRecord[]))
                {
                    LogErrorAndAddToDebugInfoForClient("ObjectData is not of expected type", requestForDebug);
                    return null;
                }

                Log.Common.INFO("VDir search FINISHED. ResultCode: '{0}' - ResultTable.RowCount: '{1}'", new object[] { resourceContainer.ResultCode, searchResults.Length });
                return searchResults;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Common.ERROR(ex);
                _application?.CurrentDebugInfo?.Failed(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, "vDir search failed: " + ex.Message + "\n\rConnectionState: " + FRWClientConnector.Instance.ClientConnector.ConnectionStateInfo.ConnectionState);
                throw;
            }
            catch (Exception ex)
            {
                Log.Common.ERROR("VDir Search failed: {0}\n\rConnectionState: {1}", new object[] { ex, FRWClientConnector.Instance.ClientConnector.ConnectionStateInfo.ConnectionState });
                //TraceExtension.Error(String.Format("VDIR search fails for query: {0}", atQueryElement.ToString()), ex);
                _application?.CurrentDebugInfo?.Failed(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, "vDir search failed: " + ex.Message + "\n\rConnectionState: " + FRWClientConnector.Instance.ClientConnector.ConnectionStateInfo.ConnectionState);
            }

            TraceExtension.Info("VDir Search FINISHED (no result)");
            return null;
        }

        private bool IsWcfConnectionStateInvalid()
        {
            if (FRWClientConnector.Instance.ClientConnector.ConnectionStateInfo.ConnectionState ==
                ATConnectionState.Connected ||
                FRWClientConnector.Instance.ClientConnector.ConnectionStateInfo.ConnectionState ==
                ATConnectionState.ConnectionPending)
            {
                return false;
            }

            return true;
        }

        private void LogErrorAndAddToDebugInfoForClient(string message, string request)
        {
            Log.Common.ERROR(string.Format("VDir Search Failed: {0} - ConnectionState: '{1}' - Request: '{2}'", message, FRWClientConnector.Instance.ClientConnector.ConnectionStateInfo.ConnectionState, request));
            _application?.CurrentDebugInfo?.Failed(Models.DebugInfo.DebugInfoSection.SECTION_VDIR, string.Format("{0} - ConnectionState: '{1}'", message, FRWClientConnector.Instance.ClientConnector.ConnectionStateInfo.ConnectionState));
        }

        MvcApplication _application
        {
            get { return HttpContext.ApplicationInstance as MvcApplication; }
        }
        #endregion

    }
}
