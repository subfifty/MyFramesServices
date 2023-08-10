using System;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.AspNetCore.Authorization;

#if POWERSHELL_CONTROLLER

namespace XPhoneRestApi.Controllers
{
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
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' ShowHelp()", client), true);
            return ShowHelp();
        }

        // GET /powershell/license
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

        // GET /powershell/scripts
        [HttpGet("scripts")]
        public object GetScripts()
        {
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
                object tryJson = JsonSerializer.Deserialize<object>(result);
                // Return as "Content-Type: application/json"
                return tryJson;
            }
            catch
            {
                // Return as "Content-Type: text/plain"
                return result;
            }
        }

        // GET /powershell/scripts/[script]?key=[key]&value=[val]
        [HttpGet("scripts/{script}")]
        public async Task<object> ExecuteScript(string script)
        {
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

                Dictionary<string, object> scriptParameters = null;

                if (key != null && value != null)
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
            catch (Exception ex) 
            { 
                result = ex.Message;
                return result; ;
            }

            try
            {
                object tryJson = JsonSerializer.Deserialize<object>(result);
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
