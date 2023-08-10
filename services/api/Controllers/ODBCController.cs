using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Data.Odbc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;

namespace XPhoneRestApi.Controllers
{
    public class OdbcRequest
    {
        public string dsn { get; set; }
        public string sql { get; set; }
        public string uid { get; set; }
        public string pwd { get; set; }
        public string opt { get; set; }
    }

    //public class OdbcResponse
    //{
    //    public OdbcResponse()
    //    {
    //        result = "failed";
    //    }
    //    public string result { get; set; }
    //    public string data { get; set; }
    //}

    [Route("[controller]")]
    [ApiController]
    [Authorize("odbc")]
    public class ODBCController : XPhoneControllerBase
    {
        private static string ControllerName = "odbc";
        private static LicenseObject ControllerLicense = ApiLicense.Instance.ParseLicenseObject("ODBC");

        // GET /odbc
        [HttpGet]
        [AllowAnonymous]
        public string GetHelp()
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' ShowHelp()", client), true);
            return ShowHelp();
        }

        // GET /odbc/license
        [HttpGet("license")]
        public JsonResult GetLicense()
        {
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

        // POST /odbc
        [HttpPost]
        public ContentResult Post([FromBody] object value)
        {
            if (!IsValidLicense())
            {
                this.Response.Headers.Add("Content-Type", "application/json");
                return this.Content("Invalid license found.", "application/json");
            }

            OdbcRequest request = System.Text.Json.JsonSerializer.Deserialize<OdbcRequest>(value.ToString());

            string result = "failed";
            string dsn = request.dsn;
            string sql = request.sql;
            string uid = request.uid;
            string pwd = request.pwd;
            string opt = request.opt;
            string data = "";
            //object data = null;

            bool autojson = false;
            if ( opt != null )
            {
                if ( opt.ToLower().Contains("autojson") )
                {
                    autojson = true;
                }
            }

            if ( autojson )
            {
                if (!sql.EndsWith("FOR JSON AUTO"))
                {
                    sql += " FOR JSON AUTO";
                }
            }

            ApiConfig.Instance.ReloadConfiguration();
            string uid_config = ApiConfig.Instance.ReadAttributeValue("odbc/" + dsn.ToLower(), "uid");
            string pwd_config = ApiConfig.Instance.ReadAttributeValue("odbc/" + dsn.ToLower(), "pwd");

            if (!String.IsNullOrEmpty(uid_config) && !String.IsNullOrEmpty(pwd_config))
            {
                uid = uid_config;
                pwd = pwd_config;
            }

            string connectionstring = "";
            if (dsn != null) connectionstring += "DSN=" + dsn + ";";
            if (uid != null) connectionstring += "UID=" + uid + ";";
            if (pwd != null) connectionstring += "PWD=" + pwd + ";";

            using (OdbcConnection connection = new OdbcConnection(connectionstring))
            {
                using (OdbcCommand command = connection.CreateCommand())
                {
                    try
                    {
                        connection.Open();
                        command.CommandText = sql;

                        var jsonResult = new StringBuilder();
                        var reader = command.ExecuteReader();

                        if (!reader.HasRows)
                        {
                            jsonResult.Append("[]");
                        }
                        else
                        {
                            if ( autojson )
                            {
                                while (reader.Read())
                                {
                                    jsonResult.Append(reader.GetValue(0).ToString());
                                }
                                data = jsonResult.ToString();
                            }
                            else
                            {
                                var r = Serialize(reader);
                                data = JsonConvert.SerializeObject(r, Formatting.None);
                            }
                        }

                        result = "success";

                        reader.Close();
                        //command.Dispose();
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            string cmd = "POST";
            string msg = string.Format("INF remoteIP='{0}' cmd='{1}' dsn='{2}' sql='{3}' result='{4}'",
                    client,
                    cmd,
                    dsn != null ? dsn : "null",
                    sql != null ? sql : "null",
                    result);
            logFile.Append(msg, true);

            this.Response.Headers.Add("Content-Type", "application/json");
            return this.Content(data, "application/json");
        }

        private string ShowHelp()
        {
            string info = "XPhone Connect ODBC API" + "\r\n" + "\r\n";

            string help =
                  @"GET  /odbc" + "\r\n"
                + @"     Show help." + "\r\n"
                + @"GET  /odbc/license" + "\r\n"
                + @"     Show license info." + "\r\n"
                + @"POST /odbc" + "\r\n"
                + @"     Complex SQL Query to ODBC DSN. Request Body:" + "\r\n"
                + @"     {" + "\r\n"
                + @"        'dsn': '<DSN datasource name'," + "\r\n"
                + @"        'sql': '<select statement>'," + "\r\n"
                + @"        'uid': '[<dsn credential: user name>]'," + "\r\n"
                + @"        'pwd': '[<dsn credential: password>]'," + "\r\n"
                + @"        'opt': 'autojson'" + "\r\n"
                + @"     }"
                ;

            if (!IsValidLicense())
            {
                help += "\r\n\r\n" + "INVALID LICENSE FOUND!";
            }

            return info + help; ;
        }

        private IEnumerable<Dictionary<string, object>> Serialize(OdbcDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(SerializeRow(cols, reader));

            return results;
        }
        
        private Dictionary<string, object> SerializeRow(IEnumerable<string> cols, OdbcDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
            {
                var d = reader[col];
                string s = d?.ToString();
                result.Add(col, s);
            }
            return result;
        }

        private bool IsValidLicense()
        {
            return ControllerLicense.valid;
        }
    }
}
