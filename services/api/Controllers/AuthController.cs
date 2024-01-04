using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XPhoneRestApi.Controllers
{
    public class RefreshResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class RefreshResponseWebapi
    {
        /*
        {
            "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJmYTI4ZjdjYi0wYTdjLTQxNGUtODVjYS00NjdjN2QwOTA1MWEiLCJzZXNzaW9uZ3VpZCI6Ijk5NTJiZjYwLTkyZTYtNGRmNS1hMmU5LTBmNzBkMWM2ZGFmNCIsIm5iZiI6MTcwNDM4MzUyOSwiZXhwIjoxNzA0Mzg1MzI5LCJpYXQiOjE3MDQzODM1Mjl9.Cpj4ZZflYYAMo1ZM-R0w-GzxWcIYopYq7YEzVbMDPw0",
            "tokenExpiresAt": 1704385329486,
            "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJNZUVhWVJXR3ZOVTJLWWVJVVJqMGlpVDlUTnhyaGJycHdLbjhEMjcyOUIwPSIsInNlc3Npb25ndWlkIjoiOTk1MmJmNjAtOTJlNi00ZGY1LWEyZTktMGY3MGQxYzZkYWY0IiwibmJmIjoxNzA0MzgzNTI5LCJleHAiOjE3MDQ3MjkxMjksImlhdCI6MTcwNDM4MzUyOX0.qtYefYumEEXEc0H51xCBtMh-zoUU6_-SIcywefOL78g",
            "refreshTokenExpiresAt": 1704729129486
        }        
        */
        public string token { get; set; }
        public string refreshToken { get; set; }
        public long tokenExpiresAt { get; set; }
        public long refreshTokenExpiresAt { get; set; }
    }

    public class RefreshRequest
    {
        /*
            body.RefreshToken = LS.GetRefreshToken();
            body.AccessToken = LS.GetAccessToken();
            body.SessionGUID = LS.GetSessionGUID();
            body.ClientSecret = "";
        */
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string SessionGUID { get; set; }
        public string ClientSecret { get; set; }
    }

    public class RefreshRequestWebapi
    {
        /*
        {
            "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJmYTI4ZjdjYi0wYTdjLTQxNGUtODVjYS00NjdjN2QwOTA1MWEiLCJzZXNzaW9uZ3VpZCI6IjUxMDg5OTBkLWZiNjEtNDEyNS04OTY1LTgxOWFiMTlhNzA1MSIsIm5iZiI6MTcwNDM4MDY3NywiZXhwIjoxNzA0MzgyNDc3LCJpYXQiOjE3MDQzODA2Nzd9.MNHabrEZVBmiP6xQKeLtosYQoTz708C0Qu5_PKICTnc",
            "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJqSzU5TjR2T2dvL2NYbjdhV0hYaGNmRy9zUVFvNzZVdGhzY09CV0t6blNjPSIsInNlc3Npb25ndWlkIjoiNTEwODk5MGQtZmI2MS00MTI1LTg5NjUtODE5YWIxOWE3MDUxIiwibmJmIjoxNzA0MzgwNjc3LCJleHAiOjE3MDQ3MjYyNzcsImlhdCI6MTcwNDM4MDY3N30.3bIYTSStmh8cmFkKT9XHkzRSrESREdy277DB2He-pI0",
            "sessionGuid": "5108990d-fb61-4125-8965-819ab19a7051"
        }
        */
        public string token {  get; set; }
        public string refreshToken { get; set; }
        public string sessionGuid { get; set; } 
    }

    public class AuthRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    public class AuthRequestWebapi
    {
        public string AccountName { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponseWebapi_UserData
    {
        public string guid {  get; set; }
        //"token": "",
        //"isGuest": false,
        public string language { get; set; }    
        public string userName { get; set; }
        public string displayName { get; set; } 
        public string distinguishedName { get; set; }
        //"title": "Ober-Test-Guru",
        public string firstName { get; set; }
        public string lastName { get; set; }
        //"companyName": "Test AG",
        //"companyAddressCity": "München",
        //"department": "Einkauf",
        //"photo": "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",
        //"phoneNumberOffice": "+49(89)8866-555",
        //"phoneNumberHomeOffice": "",
        //"phoneNumberMobile": "015155661200",
        //"phoneNumberPrivate": "",
        //"faxNumberOffice": "+49(89)8866-13555",
        //"faxNumberOther": "",
        public string emailOffice { get; set; } 
        //"emailOther": ""
    }
    public class AuthResponseWebapi_TokenData
    {
        public string token {  get; set; }
        public long tokenExpiresAt { get; set; }
        public string refreshToken { get; set; }
        public long refreshTokenExpiresAt { get; set; }  
    }
    public class AuthResponseWebapi
    {
        /*
        {
            "result": 0,
            "userData": {
                "guid": "fa28f7cb-0a7c-414e-85ca-467c7d09051a",
                "token": "",
                "isGuest": false,
                "language": "de",
                "userName": "test@v9.subfifty.de",
                "displayName": "555 Test (Sek 2)",
                "distinguishedName": "cn=555 Test \\(Sek 2\\) fa28f7cb-0a7c-414e-85ca-467c7d09051a, ou=CG c0e176c6-efd6-426b-8eb7-892db1d5231f, ou=Branch 536b920b-5bb3-4991-9584-dc2cdbac4aca, ou=Organisation",
                "title": "Ober-Test-Guru",
                "firstName": "555",
                "lastName": "Test (Sek 2)",
                "companyName": "Test AG",
                "companyAddressCity": "München",
                "department": "Einkauf",
                "photo": "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",
                "phoneNumberOffice": "+49(89)8866-555",
                "phoneNumberHomeOffice": "",
                "phoneNumberMobile": "015155661200",
                "phoneNumberPrivate": "",
                "faxNumberOffice": "+49(89)8866-13555",
                "faxNumberOther": "",
                "emailOffice": "Test@sap.com",
                "emailOther": ""
            },
            "tokenData": {
                "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJmYTI4ZjdjYi0wYTdjLTQxNGUtODVjYS00NjdjN2QwOTA1MWEiLCJzZXNzaW9uZ3VpZCI6Ijg0OGUyYzdlLWVlMTAtNGFiYS1iYzg1LTU0NTIyZTQ3ZWEzMyIsIm5iZiI6MTcwNDM2MzY3NCwiZXhwIjoxNzA0MzY1NDc0LCJpYXQiOjE3MDQzNjM2NzR9.Ipdi4QKnduosNaKepsMqPwsWyldhBEW70gbdHTSSFQo",
                "tokenExpiresAt": 1704365474605,
                "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJJVmxPTGxkQXhuU0NqZVd4dzFzQS9PemVxQjZZRUprUU5ETjN1RlovbnlzPSIsInNlc3Npb25ndWlkIjoiODQ4ZTJjN2UtZWUxMC00YWJhLWJjODUtNTQ1MjJlNDdlYTMzIiwibmJmIjoxNzA0MzYzNjc0LCJleHAiOjE3MDQ3MDkyNzQsImlhdCI6MTcwNDM2MzY3NH0.MMkCJEZWLSz1sfKsIi-F5QzFHKOAzlFPpoRJR4HlRVM",
                "refreshTokenExpiresAt": 1704709274605
            },
            "sessionGuid": "848e2c7e-ee10-4aba-bc85-54522e47ea33"
        }
         */
        public int result {  get; set; }
        public AuthResponseWebapi_UserData userData { get; set; }
        public AuthResponseWebapi_TokenData tokenData { get; set; }
        public string sessionGuid { get; set; } 
    }

    public class AuthResponse
    {
        /*
                {
                    "IsAuthenticated": true,
                    "IsAdmin": false,
                    "SessionGUID": "00000000-0000-0000-0000-000000000000",
                    "UserGUID": "bfa2034e-9509-4540-8ca4-313999d3befa",
                    "CultureInfoName": "de",
                    "FullName": "556 Mate (Chef)",
                    "AccessToken": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MDQzNjY0NzAsIklkZW50aXR5VG9rZW4iOiIxNzIyMTkzNjc5IiwiVXNlckd1aWQiOiJiZmEyMDM0ZS05NTA5LTQ1NDAtOGNhNC0zMTM5OTlkM2JlZmEiLCJGdWxsTmFtZSI6IjU1NiBNYXRlIChDaGVmKSIsIlVzZXJOYW1lIjoibWF0ZUB2OS5zdWJmaWZ0eS5kZSJ9.WlJ984OtIDmfGUS2HT9Gn-vs146rAd-bXTDErtQ5gd0",
                    "RefreshToken": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MTU0MjM4NzAsIklkZW50aXR5VG9rZW4iOiIxNzIyMTkzNjc5IiwiVXNlckd1aWQiOiJiZmEyMDM0ZS05NTA5LTQ1NDAtOGNhNC0zMTM5OTlkM2JlZmEiLCJDbGllbnRTZWNyZXQiOiJZb3VOZXZlcldhbGtBbG9uZSIsIkZ1bGxOYW1lIjoiNTU2IE1hdGUgKENoZWYpIiwiVXNlck5hbWUiOiJtYXRlQHY5LnN1YmZpZnR5LmRlIn0.b458qJuIPW22MEXjDTZCoR2hNZDOt8mQu1pWi32_Kew",
                    "IdentityToken": "1722193679",
                    "Error": "",
                    "UserName": "mate@v9.subfifty.de",
                    "Tenant": ""
                }
        */
        public AuthResponse() 
        {
            SessionGUID = "";
            UserGUID = "";
            CultureInfoName = "";
            FullName = "";
            AccessToken = "";
            RefreshToken = "";
            IdentityToken = "";
            Error = "";
            UserName = "";
            Tenant = "";
        }
        public bool IsAuthenticated { get; set; }
        public bool IsAdmin { get; set; }
        public string SessionGUID { get; set; }
        public string UserGUID { get; set; }
        public string CultureInfoName { get; set; }
        public string FullName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdentityToken { get; set; }
        public string UserName { get; set; }
        public string Error { get; set; }
        public string Tenant { get; set; }
    }

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
            string endpoint;

            if (ApiConfig.Instance.RunningInDMZ())
            {
                endpoint = ApiConfig.Instance.ReadAttributeValue("dmz", "AuthEndpoint");
                return await RelayHttp_POST("/logon", endpoint);
            }

            if ( ApiConfig.Instance.UseWebapi() )
            {
                // Convert request
                string body = await Request.GetRawBodyAsync();
                AuthRequest authRequest = System.Text.Json.JsonSerializer.Deserialize<AuthRequest>(body);

                var authRequestWebapi = new AuthRequestWebapi();
                authRequestWebapi.AccountName = authRequest.username;
                authRequestWebapi.Password = authRequest.password;

                body = JsonConvert.SerializeObject(authRequestWebapi);

                endpoint = ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpointWebApi");
                var content = await RelayHttp_POST("/user/authenticate", endpoint, body);

                // Convert response
                AuthResponseWebapi webapi = System.Text.Json.JsonSerializer.Deserialize<AuthResponseWebapi>(content.Content);

                var response = new AuthResponse();
                response.IsAuthenticated = webapi.result == 0;
                response.IsAdmin = webapi.userData.distinguishedName.Contains("cn=_Administrators");
                response.AccessToken = webapi.tokenData.token;
                response.RefreshToken = webapi.tokenData.refreshToken;
                response.UserGUID = webapi.userData.guid;
                response.FullName = webapi.userData.displayName;
                response.UserName = webapi.userData.userName;
                response.CultureInfoName = webapi.userData.language;
                response.SessionGUID = webapi.sessionGuid;

                if ( response.IsAdmin )
                {
                    response.UserName = authRequest.username;
                }

                var result = new ContentResult();
                result.ContentType = "application/json";
                result.Content = JsonConvert.SerializeObject(response);
                return result;
            }
#if DEBUG
            endpoint = "http://localhost:8080/auth";
#endif
            endpoint = ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpoint");
            return await RelayHttp_POST("/logon", endpoint);
        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            ApiConfig.Instance.ReloadConfiguration();
            string endpoint;

            if (ApiConfig.Instance.RunningInDMZ())
            {
                endpoint = ApiConfig.Instance.ReadAttributeValue("dmz", "AuthEndpoint");
                return await RelayHttp_POST("/refresh", endpoint);
            }

            if (ApiConfig.Instance.UseWebapi())
            {
                // Convert request
                string body = await Request.GetRawBodyAsync();
                RefreshRequest refreshRequest = System.Text.Json.JsonSerializer.Deserialize<RefreshRequest>(body);

                var refreshRequestWebapi = new RefreshRequestWebapi();
                refreshRequestWebapi.token = refreshRequest.AccessToken;
                refreshRequestWebapi.refreshToken = refreshRequest.RefreshToken;
                refreshRequestWebapi.sessionGuid = refreshRequest.SessionGUID;

                body = JsonConvert.SerializeObject(refreshRequestWebapi);

                endpoint = ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpointWebApi");
                var content = await RelayHttp_POST("/user/refreshToken", endpoint, body);

                // Convert response
                RefreshResponseWebapi webapi = System.Text.Json.JsonSerializer.Deserialize<RefreshResponseWebapi>(content.Content);

                var response = new RefreshResponse();
                response.AccessToken = webapi.token;
                response.RefreshToken = webapi.refreshToken;

                var result = new ContentResult();
                result.ContentType = "application/json";
                result.Content = JsonConvert.SerializeObject(response);
                return result;
            }
#if DEBUG
            endpoint = "http://localhost:8080/auth";
#endif
            endpoint = ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpoint");
            return await RelayHttp_POST("/refresh", endpoint);
        }

        [HttpPost("Verify")]
        public async Task<IActionResult> Verify()
        {
            ApiConfig.Instance.ReloadConfiguration();
            string endpoint;

            if (ApiConfig.Instance.RunningInDMZ())
            {
                endpoint = ApiConfig.Instance.ReadAttributeValue("dmz", "AuthEndpoint");
                return await RelayHttp_POST("/verify", endpoint);
            }

            if (ApiConfig.Instance.UseWebapi())
            {
                endpoint = ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpointWebApi");
                return RelayHttp_GET("/configuration/getCollaborationSection", endpoint);
            }
#if DEBUG
            endpoint = "http://localhost:8080/auth";
#endif
            endpoint = ApiConfig.Instance.ReadAttributeValue("authorization", "AuthEndpoint");
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
