using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace XPhoneRestApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize("auth")]
    public class AuthController : XPhoneControllerBase
    {
        private static string ControllerName = "auth";
        private static LicenseObject ControllerLicense = ApiLicense.Instance.ParseLicenseObject("auth");

        // GET /auth
        [HttpGet]
        [AllowAnonymous]
        public string GetHelp()
        {
            if (ApiConfig.Instance.RunningInDMZ())
                return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' ShowHelp()", client), true);
            return ShowHelp();
        }

        // GET /auth/license
        [HttpGet("license")]
        public JsonResult GetLicense()
        {
            if (ApiConfig.Instance.RunningInDMZ())
                return new JsonResult( ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ );

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetLicense()", client), true);

            LicenseInfo license = new LicenseInfo();

            license.license = ControllerLicense;
            license.customer = ApiLicense.Instance.CustomerInfo;
            license.partner = ApiLicense.Instance.PartnerInfo;
            license.package = ApiLicense.Instance.PackageInfo;

            return new JsonResult(license);
        }

        [HttpPost("Logon")]
        [AllowAnonymous]
        public async Task<IActionResult> Logon()
        {
            ApiConfig.Instance.ReloadConfiguration();
            string endpoint = ApiConfig.Instance.RunningInDMZ()
                ? ApiConfig.Instance.ReadAttributeValue("dmz", "AuthEndpoint")
                : ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpoint");
#if DEBUG
            endpoint = "http://localhost:8080/auth";
#endif

            return await RelayHttp_POST("/logon", endpoint);
        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            ApiConfig.Instance.ReloadConfiguration();
            string endpoint = ApiConfig.Instance.RunningInDMZ()
                ? ApiConfig.Instance.ReadAttributeValue("dmz", "AuthEndpoint")
                : ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpoint");
#if DEBUG
            endpoint = "http://localhost:8080/auth";
#endif

            return await RelayHttp_POST("/refresh", endpoint);
        }

        [HttpPost("Verify")]
        public async Task<IActionResult> Verify()
        {
            ApiConfig.Instance.ReloadConfiguration();
            string endpoint = ApiConfig.Instance.RunningInDMZ()
                ? ApiConfig.Instance.ReadAttributeValue("dmz", "AuthEndpoint")
                : ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpoint");
#if DEBUG
            endpoint = "http://localhost:8080/auth";
#endif

            return await RelayHttp_POST("/verify", endpoint);
        }

        private string ShowHelp()
        {
            string info = "XPhone Connect Authorization API" + "\r\n" + "\r\n";

            string help =
                  @"GET   /auth" + "\r\n"
                + @"      Show help." + "\r\n"
                + @"GET   /auth/license" + "\r\n"
                + @"      Show license info." + "\r\n"
                + @"POST  /auth/logon" + "\r\n"
                + @"      Logon with XPhone Credentials." + "\r\n"
                + @"POST  /auth/verify" + "\r\n"
                + @"      Verify access token." + "\r\n"
                + @"POST  /auth/refresh" + "\r\n"
                + @"      Refresh access token." + "\r\n"
                ;

            if (!IsValidLicense())
            {
                help += "\r\n\r\n" + "INVALID LICENSE FOUND!";
            }

            return info + help; ;
        }

        private bool IsValidLicense()
        {
            return true;
            //return ControllerLicense.valid;
        }
    }
}
