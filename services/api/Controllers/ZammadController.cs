using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;

namespace XPhoneRestApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize("zammad")]
    [AllowAnonymous]
    public class ZammadController : XPhoneControllerBase
    {
        private static string ControllerName = "zammad";

        // GET /zammad
        [HttpGet]
        public string GetHelp()
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' ShowHelp()", client), true);
            return ShowHelp();
        }

        // GET /zammad/users/{query}
        [HttpGet("user/{mail}")]
        public ContentResult GetUser(string mail)
        {

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetUser()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");

            //return Execute_GET(@"/users/search?query=" + query);
            return Execute_GET(@"/users/search?query=email:" + mail);//done for Zammad
        }

        [HttpGet("users/{ids}")]
        public ContentResult GetManyUsers(string ids)
        {

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetManyUsers()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");
            string request = @"/users/search?query=(";
            bool first = true;
            foreach (string id in ids.Split(","))
            {
                if (!first) {
                    request += " OR ";
                }
                request += "id:" + id;
                first = false;
            }
            request += ")";
            //return Execute_GET(@" / users/search?query=" + query);
            return Execute_GET(request);
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
            string query = @"/tickets/search?query=organization.id:";
            query += id;

            if (sortCriteria != null && !sortCriteria.Equals(""))
            {
                if (sortCriteria == "status")
                {
                    query += "&sort_by=state_id";
                }
                else if (sortCriteria == "created_at")
                {
                    query += "&sort_by=created_at";
                }
            }
            if (sortOrder != null && !sortOrder.Equals(""))
            {
                query += "&order_by=" + sortOrder;
            }
            if (limit != null && !limit.Equals(""))
            {
                query += "&limit=" + limit;
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
            string query = @"/tickets/search?query=";
            query += fieldId + ":" + fieldValue;
            if (sortCriteria != null && !sortCriteria.Equals(""))
            {
                if (sortCriteria == "status")
                {
                    query += "&sort_by=state_id";
                }
                else if (sortCriteria == "created_at")
                {
                    query += "&sort_by=created_at";
                }
            }
            if (sortOrder != null && !sortOrder.Equals(""))
            {
                query += "&order_by=" + sortOrder;
            }
            if (limit != null && !limit.Equals(""))
            {
                query += "&limit=" + limit;
            }
            return Execute_GET(query);
        }

        [HttpGet("tickets/requester")]
        public ContentResult GetRequester(string id, string sortCriteria, string sortOrder, string limit)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetRequester()", client), true);

            this.Response.Headers.Add("Content-Type", "application/json");
            string query = @"/tickets/search?query=customer_id:";
            query += id;
            if (sortCriteria != null && !sortCriteria.Equals(""))
            {
                if (sortCriteria == "status")
                {
                    query += "&sort_by=state_id";
                }
                else if (sortCriteria == "created_at")
                {
                    query += "&sort_by=created_at";
                }
            }
            if (sortOrder != null && !sortOrder.Equals(""))
            {
                query += "&order_by=" + sortOrder;
            }
            if (limit != null && !limit.Equals(""))
            {
                query += "&limit=" + limit;
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
            string Base_URL = ApiConfig.Instance.ReadAttributeValue("zammad", "BaseUrl");

            return this.Content(@"{""url"":"""+ Base_URL + @"""}", "application/json");
        }

        private string ShowHelp()
        {
            string info = "XPhone Connect ZAMMAD API" + "\r\n" + "\r\n";

            string help =
                  @"GET /zammad" + "\r\n"
                + @"    Show help." + "\r\n"
                ;

            string helpDeprecated = ""
                ;

            return info + help + "\r\n" + helpDeprecated; ;
        }

        private ContentResult Execute_GET(string query = "")
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return this.Content(ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ, "application/json");

            ApiConfig.Instance.ReloadConfiguration();
            string Base_API_URL = ApiConfig.Instance.ReadAttributeValue("zammad", "BaseAPIUrl");
            string Bearer_Token = ApiConfig.Instance.ReadAttributeValue("zammad", "BearerToken");

            if (String.IsNullOrEmpty(Base_API_URL) || string.IsNullOrEmpty(Bearer_Token))
            {
                return this.Content("Cannot execute query. Missing Api Parameters.", "application/json");
            }

            try
            {
                string url = Base_API_URL + query;
                //url = System.Web.HttpUtility.UrlEncode(url);

                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 10);
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Bearer_Token);
                var result = Task.Run(() => client.GetStringAsync(url)).Result;

                return this.Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return this.Content(ex.ToString(), "application/json");
            }
        }

    }
}
