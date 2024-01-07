using System;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.AspNetCore.Authorization;

#if POWERSHELL_CONTROLLER

namespace XPhoneRestApi.Controllers
{
    public class PowershellRequest
    {
        public string script { get; set; }
        public Dictionary<string, string> param { get; set; }
    }

    [Route("[controller]")]
    [ApiController]
    [Authorize("powershell")]
    public class PowershellController : XPhoneControllerBase
    {
        private static string ControllerName = "powershell";
        private static LicenseObject ControllerLicense = ApiLicense.Instance.ParseLicenseObject("Powershell");

        // GET /powershell
        [HttpGet]
        [AllowAnonymous]
        public string Get()
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' ShowHelp()", client), true);
            return ShowHelp();
        }

        // GET /powershell/license
        [HttpGet("license")]
        public JsonResult GetLicense()
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return new JsonResult(ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ);

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

        // GET /powershell/scripts
        [HttpGet("scripts")]
        public object GetScripts()
        {
            //if ( ApiConfig.Instance.RunningInDMZ() )
            //    return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            if (!IsValidLicense())
                return "License not valid.";

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' GetScripts()", client), true);

            string result = "[";
            string psDir = PowershellControlDirectory;
            foreach (string psFile in Directory.GetFiles(psDir, "*.ps1", SearchOption.TopDirectoryOnly))
            {
                string psName = Path.GetFileName(psFile);
                if (result != "[")
                    result += ",";
                result += "\r\n  { \"Script\" : \"" + psName + "\" }";
            }
            result += "\r\n]";

            try
            {
                object tryJson = System.Text.Json.JsonSerializer.Deserialize<object>(result);
                // Return as "Content-Type: application/json"
                return tryJson;
            }
            catch
            {
                // Return as "Content-Type: text/plain"
                return result;
            }
        }

        // GET /powershell/noauth/[script]?key=[key]&value=[val]
        [HttpGet("noauth/{script}")]
        [AllowInDMZ]
        [AllowAnonymous]
        public async Task<object> ExecuteScriptNoAuth(string script)
        {
            if ( ApiConfig.Instance.RunningInDMZ() )
            {
                return Relay_ApiEndpoint_GET();
            }

            if (!IsValidLicense())
                return "License not valid.";

            string result = "Script requires authorizisation: " + script;

            switch (script.ToLower() )
            {
                case "get-xphonedomain":
                    break;
                default:
                    return result;
            }

            return await ExecuteScript(script);
        }

        // GET /powershell/scripts/[script]?key=[key]&value=[val]
        [HttpGet("scripts/{script}")]
        [AllowInDMZ]
        public async Task<object> ExecuteScript(string script)
        {
            if (ApiConfig.Instance.RunningInDMZ())
            {
                return Relay_ApiEndpoint_GET();
            }

            if (!IsValidLicense())
                return "License not valid.";

            string result = "Invalid script.";

            try
            {
                if (script == null)
                    return result;

                string key = null;
                string value = null;
                if (Request.Query.ContainsKey("key"))
                {
                    if (Request.Query.ContainsKey("value"))
                    {
                        key = Request.Query["key"];
                        value = Request.Query["value"];
                    }
                }

                LogFile logFile = Logfiles.Find(ControllerName);
                string client = GetRemoteIPAddress().ToString();
                logFile.Append(string.Format("INF remoteIP='{0}' ExecuteScript('{1}')", client, script), true);

                Dictionary<string, object> scriptParameters = new Dictionary<string, object>();

                if (key != null && value != null)
                {
                    scriptParameters.Add(key, value);
                }

                ApiConfig.Instance.ReloadConfiguration();
                string sqlHost = ApiConfig.Instance.ReadAttributeValue(ControllerName, "sqlHost");
                string sqlDB = ApiConfig.Instance.ReadAttributeValue(ControllerName, "sqlDB");
                string sqlUid = ApiConfig.Instance.ReadAttributeValue(ControllerName, "sqlUid");
                string sqlPwd = ApiConfig.Instance.ReadAttributeValue(ControllerName, "sqlPwd");

                if (!string.IsNullOrEmpty(sqlHost) && !scriptParameters.ContainsKey("sqlHost")) scriptParameters.Add("sqlHost", sqlHost);
                if (!string.IsNullOrEmpty(sqlDB) && !scriptParameters.ContainsKey("sqlDB")) scriptParameters.Add("sqlDB", sqlDB);
                if (!string.IsNullOrEmpty(sqlUid) && !scriptParameters.ContainsKey("sqlUid")) scriptParameters.Add("sqlUid", sqlUid);
                if (!string.IsNullOrEmpty(sqlPwd) && !scriptParameters.ContainsKey("sqlPwd")) scriptParameters.Add("sqlPwd", sqlPwd);

                string path = Path.Combine(PowershellControlDirectory, script + ".ps1");
                string psScript = System.IO.File.ReadAllText(path);
                result = await RunScript(psScript, scriptParameters);
            }
            catch (Exception ex) 
            { 
                result = ex.Message;
                return result; ;
            }

            try
            {
                object tryJson = System.Text.Json.JsonSerializer.Deserialize<object>(result);
                // Return as "Content-Type: application/json"
                return tryJson;
            }
            catch
            {
                // Return as "Content-Type: text/plain"
                return result;
            }
        }

        // POST /powershell/execute
        [HttpPost("execute")]
#if DEBUG
        [AllowAnonymous]
#endif
        public async Task<object> ExecuteScriptPOST([FromBody] object body)
        {
            if (!IsValidLicense())
                return "License not valid.";

            PowershellRequest request = System.Text.Json.JsonSerializer.Deserialize<PowershellRequest>(body.ToString());

            string result = "failed";
            string script = request.script;
            Dictionary<string, string> param = request.param;

            try
            {
                if (string.IsNullOrEmpty(script))
                    return result;

                LogFile logFile = Logfiles.Find(ControllerName);
                string client = GetRemoteIPAddress().ToString();
                logFile.Append(string.Format("INF remoteIP='{0}' Execute('{1}')", client, script), true);

                Dictionary<string, object> scriptParameters = new Dictionary<string, object>();
                if (param != null) 
                {
                    foreach (var p in param)
                    {
                        scriptParameters.Add(p.Key, p.Value);
                    }
                }

                ApiConfig.Instance.ReloadConfiguration();
                string sqlHost = ApiConfig.Instance.ReadAttributeValue(ControllerName, "sqlHost");
                string sqlDB = ApiConfig.Instance.ReadAttributeValue(ControllerName, "sqlDB");
                string sqlUid = ApiConfig.Instance.ReadAttributeValue(ControllerName, "sqlUid");
                string sqlPwd = ApiConfig.Instance.ReadAttributeValue(ControllerName, "sqlPwd");

                if (!string.IsNullOrEmpty(sqlHost) && !scriptParameters.ContainsKey("sqlHost")) scriptParameters.Add("sqlHost", sqlHost);
                if (!string.IsNullOrEmpty(sqlDB) && !scriptParameters.ContainsKey("sqlDB")) scriptParameters.Add("sqlDB", sqlDB);
                if (!string.IsNullOrEmpty(sqlUid) && !scriptParameters.ContainsKey("sqlUid")) scriptParameters.Add("sqlUid", sqlUid);
                if (!string.IsNullOrEmpty(sqlPwd) && !scriptParameters.ContainsKey("sqlPwd")) scriptParameters.Add("sqlPwd", sqlPwd);

                string path = Path.Combine(PowershellControlDirectory, script + ".ps1");
                string psScript = System.IO.File.ReadAllText(path);
                result = await RunScript(psScript, scriptParameters);
            }
            catch (Exception ex)
            {
                result = ex.Message;
                return result; ;
            }

            try
            {
                object tryJson = System.Text.Json.JsonSerializer.Deserialize<object>(result);
                // Return as "Content-Type: application/json"
                return tryJson;
            }
            catch
            {
                // Return as "Content-Type: text/plain"
                return result;
            }
        }

        // GET /powershell/[cmd]
        [HttpGet("{cmd}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<string> Get(string cmd)
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            if (!IsValidLicense())
                return "License not valid.";

            string result = "OK: cmd = " + cmd;

            LogFile logFile = Logfiles.Find(ControllerName);

            string script = null;
            string key = null;
            string value = null;

            switch (cmd)
            {
                #region cmd=list
                case "list":
                    string txt = "[";
                    string psDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "C4B\\Powershell");
                    foreach( string psFile in Directory.GetFiles(psDir, "*.ps1", SearchOption.TopDirectoryOnly) )
                    {
                        string psName = Path.GetFileName(psFile);
                        if (txt != "[")
                            txt += ",";
                        txt += "\r\n  { \"Script\" : \"" + psName + "\" }";
                    }
                    txt += "\r\n]";
                    result = txt;
                    break;
                #endregion

                #region cmd=execute
                case "execute":
                    if (Request.Query.ContainsKey("script"))
                    {
                        script = Request.Query["script"];
                    }
                    if (Request.Query.ContainsKey("key"))
                    {
                        if (Request.Query.ContainsKey("value"))
                        {
                            key = Request.Query["key"];
                            value = Request.Query["value"];
                        }
                    }

                    if (script != null)
                    {
                        Dictionary<string, object> scriptParameters = null;

                        if ( key != null && value != null)
                        {
                            scriptParameters = new Dictionary<string, object>()
                            {
                                { key, value }
                            };
                        }
                        string path = Path.Combine(PowershellControlDirectory, script + ".ps1");
                        string psScript = System.IO.File.ReadAllText(path);
                        result = await RunScript(psScript, scriptParameters);
                    }
                    break;
                    #endregion
            }

            string msg = string.Format("cmd='{0}' name='{1}'", cmd, script != null ? script : "null");
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' {1}", client, msg), true);

            return result;
        }

        //private static string PowershellControlFileName
        //{
        //    get
        //    {
        //        string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"C4B\Powershell\PowershellControl.Ini");
        //        string dir = Path.GetDirectoryName(path);
        //        Directory.CreateDirectory(dir);
        //        return path;
        //    }
        //}

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
        public async Task<string> RunScript(string scriptContents, Dictionary<string, object> scriptParameters)
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

                    // execute the script and await the result.
                    var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);

                    // collect the resulting pipeline objects and return 
                    result = "";
                    foreach (var item in pipelineObjects)
                    {
                        result += item.BaseObject.ToString();
                    }
                }
            }
            catch( Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        private string ShowHelp()
        {
            string info = "XPhone Connect Powershell API" + "\r\n" + "\r\n";

            string help =
                  @"GET /powershell" + "\r\n"
                + @"    Show help." + "\r\n"
                + @"GET /powershell/license" + "\r\n"
                + @"    Show license info." + "\r\n"
                + @"GET /powershell/scripts" + "\r\n"
                + @"    List all available powershell scripts on the server." + "\r\n"
                + @"GET /powershell/scripts/[script]?key=[key]&value=[val]" + "\r\n"
                + @"    Execute named [script] with optional parameter." + "\r\n"
                + @"GET /powershell/noauth/[script]?key=[key]&value=[val]" + "\r\n"
                + @"    Execute named [script] always anonymously." + "\r\n"
                + @"POST /powershell/execute" + "\r\n"
                + @"    Execute script as defined in request body." + "\r\n"
                ;

            string helpDeprecated =
                  @"DEPRECATED API:" + "\r\n"
                + @"/Powershell/{cmd}[?{request params}]" + "\r\n"
                + @"{cmd} execute, list" + "\r\n"
                + @"   execute?script={ps-name without extension .ps1}&key={parameter-name}&value={paramenter-value}" + "\r\n"
                + @"   list" + "\r\n"
                ;

            if (!IsValidLicense())
            {
                help += "\r\n\r\n" + "INVALID LICENSE FOUND!" + "\r\n\r\n";
            }

            return info + help + "\r\n" + helpDeprecated; ;
        }

        private bool IsValidLicense()
        {
            return ControllerLicense.valid;
        }

    }
}

#endif // POWERSHELL_CONTROLLER
