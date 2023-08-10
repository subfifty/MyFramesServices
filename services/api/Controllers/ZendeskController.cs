using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace XPhoneRestApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ZendeskController : XPhoneControllerBase
    {
        private static string ControllerName = "zendesk";

        // GET /zendesk
        [HttpGet]
        public string GetHelp()
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' ShowHelp()", client), true);
            return ShowHelp();
        }

        // GET /zendesk/user/{mail}
        [HttpGet("user/{mail}")]
        public ContentResult GetUser(string mail)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetUser()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");

            //return Execute_GET(@"/users/search?query=" + query);
            return Execute_GET(@"/search?query=type:user """+ mail + @"""&sort_by=created_at");
        }

        [HttpGet("users/{ids}")]
        public ContentResult GetManyUsers(string ids)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetManyUsers()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");

            //return Execute_GET(@"/users/search?query=" + query);
            return Execute_GET(@"/users/show_many?ids=" + ids);
        }

        [HttpGet("organization/{id}")]
        public ContentResult GetOrganizations(string id)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetOrganizations()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");

            return Execute_GET(@"/organizations/" + id);
        }

        [HttpGet("tickets/organization")]
        public ContentResult GetOrganization(string id, string sortCriteria, string sortOrder, string limit)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetOrganization()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");
            string query = @"/search?query=organization:";
            query += id + " type:ticket";

            if (sortCriteria != null && !sortCriteria.Equals(""))
            {
                query += "&sort_by=" + sortCriteria;
            }
            if (sortOrder != null && !sortOrder.Equals(""))
            {
                query += "&sort_order=" + sortOrder;
            }
            if (limit != null && !limit.Equals(""))
            {
                query += "&per_page=" + limit;
            }
            return Execute_GET(query);
        }

        [HttpGet("tickets/custom")]
        public ContentResult GetCustom(string fieldId, string fieldValue, string sortCriteria, string sortOrder, string limit)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetCustom()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");
            string query = @"/search?query=custom_field_";
            query += fieldId + ":" + fieldValue +" type:ticket";
            if (sortCriteria != null && !sortCriteria.Equals(""))
            {
                query += "&sort_by=" + sortCriteria;
            }
            if (sortOrder != null && !sortOrder.Equals(""))
            {
                query += "&sort_order=" + sortOrder;
            }
            if (limit != null && !limit.Equals(""))
            {
                query += "&per_page=" + limit;
            }
            return Execute_GET(query);
        }

        [HttpGet("tickets/requester")]
        public ContentResult GetRequester(string mail, string sortCriteria, string sortOrder, string limit)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetRequester()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");
            string query = @"/search?query=requester:";
            query += mail + " type:ticket";

            if(sortCriteria != null && !sortCriteria.Equals(""))
            {
                query += "&sort_by=" + sortCriteria;
            }
            if (sortOrder != null && !sortOrder.Equals(""))
            {
                query += "&sort_order=" + sortOrder;
            }
            if (limit != null && !limit.Equals(""))
            {
                query += "&per_page=" + limit;
            }
            return Execute_GET(query);
        }

        [HttpGet("url")]
        public ContentResult GetBaseUrl()
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetBaseUrl()", client), true);
            this.Response.Headers.Add("Content-Type", "application/json");

            ApiConfig.Instance.ReloadConfiguration();
            string Base_URL = ApiConfig.Instance.ReadAttributeValue("zendesk", "BaseUrl");

            return this.Content(@"{""url"":""" + Base_URL + @"""}", "application/json");
        }

        private string ShowHelp()
        {
            string info = "XPhone Connect ZENDESK API" + "\r\n" + "\r\n";

            string help =
                  @"GET /zendesk" + "\r\n"
                + @"    Show help." + "\r\n"
                + @"GET /zendesk/url" + "\r\n"
                + @"    Return Zendesk service URL." + "\r\n"
                + @"GET /zendesk/user/{mail}" + "\r\n"
                + @"    Return Zendesk user(s) by mail-address or name." + "\r\n"
                ;

            string helpDeprecated = ""
                //+ @"BaseUrl = " + ApiConfig.Instance.ReadAttributeValue("zendesk", "BaseUrl") +  "\r\n"
                //+ @"Token   = " + ApiConfig.Instance.ReadAttributeValue("zendesk", "BearerToken") + "\r\n"
                ;

            return info + help + "\r\n" + helpDeprecated; ;
        }

        private ContentResult Execute_GET(string query = "")
        {
            ApiConfig.Instance.ReloadConfiguration();
            string Base_API_URL     = ApiConfig.Instance.ReadAttributeValue("zendesk", "BaseAPIUrl");
            string Bearer_Token = ApiConfig.Instance.ReadAttributeValue("zendesk", "BearerToken");

            if ( String.IsNullOrEmpty(Base_API_URL) || string.IsNullOrEmpty(Bearer_Token))
            {
                return this.Content("Cannot execute query. Missing Api Parameters.", "application/json");
            }

            try
            {
                // Beispiel: https://c4b.zendesk.com/api/v2/users/search?query=simon.gallego@telekom.de
                string url = Base_API_URL + query;
                //url = System.Web.HttpUtility.UrlEncode(url);

                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 10);
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Bearer_Token);
                var result = Task.Run(() => client.GetStringAsync(url)).Result;
                return this.Content(result, "application/json");
                //return result;
            }
            catch (Exception ex)
            {
                return this.Content(ex.ToString(), "application/json");
            }
        }
    }
}
