﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Text.Json;

namespace XPhoneRestApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize("presence")]
    public class PresenceController : XPhoneControllerBase
    {
        private static string ControllerName = "presence";

        // GET /presence/users/{mail}/agentstate/{state}
        [HttpGet("users/{mail}/agentstate/{state}")]
        [AllowInDMZ]
        public ContentResult SetAgentState(string mail, string state)
        {
            if (ApiConfig.Instance.RunningInDMZ())
            {
                return Relay_ApiEndpoint_GET();
            }

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' SetUserState({1},{2})", client, mail, state), true);

            this.Response.Headers.Add("Content-Type", "application/json");

            if (state == "on")   state = "1";
            if (state == "off")  state = "3";
            if (state == "work") state = "5";

            return Execute_GET("/" + mail + @"/edit?attribute=TeamDeskAgentState&value=" + state);
        }

        // GET /presence/users
        [HttpGet("users")]
        [AllowInDMZ]
        public ContentResult GetUsers()
        {
            if (ApiConfig.Instance.RunningInDMZ())
            {
                return Relay_ApiEndpoint_GET();
            }

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetUsers()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");

            return Execute_GET("/");
        }

        // POST /presence/users
        [HttpPost("users")]
        public ContentResult GetUserList([FromBody] object value)
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return this.Content(ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ, "application/json");

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetUserList()", client), true);

            PresenceRequest request = JsonSerializer.Deserialize<PresenceRequest>(value.ToString());

            List<object> resX = new List<object>();
            foreach ( var email in request.emails)
            {
                try
                {
                    ContentResult res = Execute_GET("/" + email);
                    var p = res.Content;
                    if ( p.StartsWith("{") )
                        resX.Add(p);
                }
                catch { }
            }
            
            PresenceResponse response = new PresenceResponse();
            response.presence = resX;

            //var result = JsonSerializer.Serialize<List<object>>(resX);
            var result = JsonSerializer.Serialize<PresenceResponse>(response);

            this.Response.Headers.Add("Content-Type", "application/json");
            return this.Content(result, "application/json");
        }

        // GET /presence/users/{mail}
        [HttpGet("users/{mail}")]
        [AllowInDMZ]
        public ContentResult GetUser(string mail)
        {
            if (ApiConfig.Instance.RunningInDMZ())
            {
                return Relay_ApiEndpoint_GET();
            }

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetUser({1})", client, mail), true);

            this.Response.Headers.Add("Content-Type", "application/json");

            //return Execute_GET(@"/users/search?query=" + query);
            return Execute_GET("/" + mail);
        }

        // GET /presence
        [HttpGet]
        [AllowAnonymous]
        public string GetHelp()
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' ShowHelp()", client), true);
            return ShowHelp();
        }

        private string ShowHelp()
        {
            ApiConfig.Instance.ReloadConfiguration();

            string info = "XPhone Connect Presence API" + "\r\n" + "\r\n";

            string help =
                  @"GET /presence" + "\r\n"
                + @"    Show help." + "\r\n"
                + @"GET /presence/users" + "\r\n"
                + @"    Return presence of all users." + "\r\n"
                + @"GET /presence/users/{mail}" + "\r\n"
                + @"    Return presence of single user by mail-address." + "\r\n"
                + @"POST /presence/users" + "\r\n"
                + @"    Return presence of multiple users by mail-address(es)." + "\r\n"
                + @"GET /presence/users/{mail}/agentstate/{state}" + "\r\n"
                + @"    Update teamdesk agent state of user by mail-address." + "\r\n"
                ;

            string helpDeprecated = ""
                + @"BaseUrl    = " + ApiConfig.Instance.ReadAttributeValue("presence", "BaseUrl") + "\r\n"
                + @"BaseAPIUrl = " + ApiConfig.Instance.ReadAttributeValue("presence", "BaseAPIUrl") +  "\r\n"
                //+ @"Email = " + "users/mate@v9.subfifty.de/agentstate/1" + "\r\n"
                //+ @"Token   = " + ApiConfig.Instance.ReadAttributeValue("presence", "BearerToken") + "\r\n"
                ;

            return info + help + "\r\n" + helpDeprecated; ;
        }

        private ContentResult Execute_GET(string query = "")
        {
            ApiConfig.Instance.ReloadConfiguration();
            string Base_API_URL = ApiConfig.Instance.ReadAttributeValue("presence", "BaseAPIUrl");

            if (String.IsNullOrEmpty(Base_API_URL))
            {
                return this.Content("Cannot execute query. Missing Api Parameters.", "application/json");
            }

            try
            {
                // Beispiel: https://c4b.zendesk.com/api/v2/users/search?query=simon.gallego@telekom.de
                string url = Base_API_URL + query;
                //url = System.Web.HttpUtility.UrlEncode(url);
                string result = "ERROR";
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 10);
                    client.DefaultRequestHeaders.Add("Accept", "*/*");

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
    }

    public class PresenceRequest
    {
        public List<string> emails { get; set; }
    }

    public class PresenceResponse
    {
        public List<object> presence { get; set; }
    }
}
