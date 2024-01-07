using System;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

#if CLIP_CONTROLLER

namespace XPhoneRestApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize("callrouting")]
    public class ClipController : XPhoneControllerBase
    {
        private static string ControllerName = "callrouting";
        private static LicenseObject ControllerLicense = ApiLicense.Instance.ParseLicenseObject("CallRouting");

        // GET /clip
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

        // GET /clip/license
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

        // GET /clip/{cmd}
        [HttpGet("{cmd}")]
        [AllowInDMZ]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public string Get(string cmd)
        {
            if (ApiConfig.Instance.RunningInDMZ())
            {
                return Relay_ApiEndpoint_GET().Content;
            }

            if (!IsValidLicense())
                return "License not valid.";

            string result = "OK: cmd = " + cmd;

            LogFile logFile = Logfiles.Find(ControllerName);

            string client = GetRemoteIPAddress().ToString();
            string agent = null;
            string partner = null;
            string clip = null;
            string newClip = null;
            string ClipOnce = null;

            switch (cmd)
            {
                #region cmd=reset
                case "reset":
                    if (Request.Query.ContainsKey("agent"))
                    {
                        agent = Normalize(Request.Query["agent"]);
                    }
                    if (agent != null)
                    {
                        IniFile ini = new IniFile(ClipControlFileName);
                        ini.WriteString(agent, null, null);
                    }
                    break;
                #endregion

                #region cmd=filter
                case "filter":
                    string whitelist = null;
                    string blacklist = null;
                    if (Request.Query.ContainsKey("whitelist"))
                    {
                        whitelist = Normalize(Request.Query["whitelist"]);
                    }
                    if (Request.Query.ContainsKey("blacklist"))
                    {
                        blacklist = Normalize(Request.Query["blacklist"]);
                    }
                    if (whitelist != null || blacklist != null)
                    {
                        IniFile ini = new IniFile(ClipControlFileName);
                        if (whitelist != null)
                        {
                            if (whitelist == "enable")
                            {
                                ini.WriteString("Filter", "Whitelist", "Enable");
                            }
                            else if (whitelist == "disable")
                            {
                                ini.WriteString("Filter", "Whitelist", "Disable");
                            }
                            else
                            {
                                ini.WriteString("Filter", whitelist, "Whitelist");
                            }
                        }
                        if (blacklist != null)
                        {
                            if (blacklist == "enable")
                            {
                                ini.WriteString("Filter", "Blacklist", "Enable");
                            }
                            else if (blacklist == "disable")
                            {
                                ini.WriteString("Filter", "Blacklist", "Disable");
                            }
                            else
                            {
                                ini.WriteString("Filter", blacklist, "Blacklist");
                            }
                        }
                    }
                    break;
                #endregion

                #region cmd=read            
                case "read":
                    string txt = System.IO.File.ReadAllText(ClipControlFileName);
                    result = txt;
                    break;
                #endregion

                #region cmd=store
                case "store":
                    if (Request.Query.ContainsKey("agent"))
                    {
                        agent = Normalize(Request.Query["agent"]);
                    }
                    if (Request.Query.ContainsKey("partner"))
                    {
                        partner = Normalize(Request.Query["partner"]);
                    }
                    if (Request.Query.ContainsKey("clip"))
                    {
                        clip = Normalize(Request.Query["clip"]);
                    }
                    else
                    {
                        clip = "";
                    }

                    if (agent != null && partner != null && clip != null)
                    {
                        IniFile ini = new IniFile(ClipControlFileName);
                        if (ini.IsValid(clip))
                        {
                            ini.WriteString(agent, partner, clip);
                        }
                    }
                    break;
                #endregion

                #region cmd=find
                case "find":
                    if (Request.Query.ContainsKey("agent"))
                    {
                        agent = Normalize(Request.Query["agent"]);
                    }
                    if (Request.Query.ContainsKey("partner"))
                    {
                        partner = Normalize(Request.Query["partner"]);
                    }
                    if (agent != null && partner != null)
                    {
                        newClip = GetClip(agent, partner);
                    }
                    if (!String.IsNullOrWhiteSpace(newClip))
                    {
                        result = newClip;
                    }
                    break;
                #endregion

                #region cmd=peek
                case "peek":
                    if (Request.Query.ContainsKey("agent"))
                    {
                        agent = Normalize(Request.Query["agent"]);
                    }
                    if (Request.Query.ContainsKey("partner"))
                    {
                        partner = Normalize(Request.Query["partner"]);
                    }
                    if (agent != null && partner != null)
                    {
                        newClip = GetClip(agent, partner, false);
                    }
                    if (!String.IsNullOrWhiteSpace(newClip))
                    {
                        result = newClip;
                    }
                    break;
                #endregion

                #region cmd=options
                case "options":
                    if (Request.Query.ContainsKey("agent"))
                    {
                        agent = Normalize(Request.Query["agent"]);
                    }
                    if (Request.Query.ContainsKey("cliponce"))
                    {
                        ClipOnce = Normalize(Request.Query["cliponce"]);
                    }
                    if (Request.Query.ContainsKey("clip"))
                    {
                        clip = Normalize(Request.Query["clip"]);
                    }
                    if (agent != null)
                    {
                        IniFile ini = new IniFile(ClipControlFileName);
                        if (ClipOnce != null)
                        {
                            ini.WriteString(agent, "ClipOnce", ClipOnce);
                        }
                        if (clip != null)
                        {
                            ini.WriteString(agent, "Clip", clip);
                        }
                    }
                    break;
                    #endregion
            }

            string msg = string.Format("INF remoteIP='{6}' cmd='{0}' From='{1}' To='{2}' clip='{3}' newClip='{4}' ClipOnce='{5}'",
                cmd,
                agent != null ? agent : "null",
                partner != null ? partner : "null",
                clip != null ? clip : "null",
                newClip != null ? newClip : "null",
                ClipOnce != null ? ClipOnce : "null",
                client);
            logFile.Append(msg, true);

            return result;
        }

        // POST <AnynodeController>
        [HttpPost("anynode/route")]
        public JsonResult Post([FromBody] object value)
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return new JsonResult(ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ);

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();

            if (!IsValidLicense())
            {
                string msgErr = string.Format("ERR remoteIP='{0}' AnyNode Route Supervision Request: Invalid XPhone REST API license found.",
                    client);
                logFile.Append(msgErr, true);

                AnynodeRoutingResponse responseErr = new AnynodeRoutingResponse();
                responseErr.routeIgnore = true;
                responseErr.routeContinue = false;
                responseErr.routeReject = true;
                responseErr.rejectReason = "Invalid XPhone REST API license found. ";
                return new JsonResult(responseErr);
            }

            string agent = null;
            string partner = null;
            string newClip = null;

            AnynodeRoutingRequest request = JsonSerializer.Deserialize<AnynodeRoutingRequest>(value.ToString());

            agent = Normalize(request.sourceAddress.dialString);
            partner = Normalize(request.destinationAddress.dialString);

            if (agent != null && partner != null)
            {
                newClip = GetClip(agent, partner);
            }

            string msg = "";
            msg = string.Format("INF remoteIP='{3}' AnyNode Route Supervision Request: From='{0}' To='{1}' Response: NewClip='{2}'",
                agent != null ? agent : "null",
                partner != null ? partner : "null",
                newClip != null ? newClip : "null",
                client);
            logFile.Append(msg + " TRYING", true);

            AnynodeRoutingResponse response = new AnynodeRoutingResponse();
            response.routeIgnore = false;
            response.routeContinue = true;
            if (!String.IsNullOrWhiteSpace(newClip))
            {
                if (newClip != agent)
                {
                    //response.routeIgnore = false;
                    //response.routeContinue = true;

                    response.sourceAddress = new SourceAddress();
                    if (newClip == "unknown")
                    {
                        response.sourceAddress.dialString = "";
                    }
                    else
                    {
                        response.sourceAddress.dialString = "+" + newClip;
                        //response.sourceAddress.dialString = newClip;    // ohne "+" für anynode training
                        response.sourceAddress.tagSet = request.sourceAddress.tagSet;
                        response.sourceAddress.displayName = request.sourceAddress.displayName;
                    }
                }
            }

            //response.routeContinue = true;
            //response.routeIgnore = false;
            //response.routeReject = false;
            //response.rejectReason = AnynodeRejectReason.success.ToString();
            //response.destinationAddress = new DestinationAddress();
            //response.destinationAddress.dialString = "";
            //response.destinationAddress.displayName = "";
            //response.sourceAddress = new SourceAddress();
            //response.sourceAddress.dialString = "";
            //response.sourceAddress.displayName = "";

            logFile.Append(msg + " DONE", true);
            return new JsonResult(response);
        }

        #region HELPER
        private static string ClipControlFileName
        {
            get
            {
                string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"C4B\Clip\ClipControl.Ini");
                string dir = Path.GetDirectoryName(path);
                Directory.CreateDirectory(dir);
                return path;
            }
        }

        static string GetClip(string a_AgentNumber, string a_CalleeNumber, bool a_DeleteClickOnce = true)
        {
            IniFile ini = new IniFile(ClipControlFileName);

            string clip = a_AgentNumber;
            string tmpClip = "";

            tmpClip = ini.ReadString(a_AgentNumber, "ClipOnce");
            if (String.IsNullOrEmpty(tmpClip))
            {
                tmpClip = ini.ReadString(a_AgentNumber, "Clip");
                if (String.IsNullOrEmpty(tmpClip))
                {
                    tmpClip = ini.ReadString(a_AgentNumber, a_CalleeNumber);
                    if (String.IsNullOrEmpty(tmpClip))
                    {
                        clip = a_AgentNumber;
                    }
                    else
                    {
                        clip = tmpClip;
                    }
                }
                else
                {
                    clip = tmpClip;
                }
            }
            else
            {
                if (a_DeleteClickOnce)
                {
                    ini.WriteString(a_AgentNumber, "ClipOnce", null);
                }
                clip = tmpClip;
            }

            //[Filter]
            //Whitelist = Enable
            //Blacklist = Enable
            //+498984079812091 = Whitelist
            //+498984079812092 = Blacklist

            if (ini.ReadString("Filter", "Blacklist") == "Enable")
            {
                if (ini.ReadString("Filter", clip) == "Blacklist")
                {
                    clip = a_AgentNumber;
                    return clip;
                }
            }

            if (ini.ReadString("Filter", "Whitelist") == "Enable")
            {
                if (ini.ReadString("Filter", clip) != "Whitelist")
                {
                    clip = a_AgentNumber;
                    return clip;
                }

            }

            return clip;
        }

        private string Normalize(string number)
        {
            string result = "";
            
            try
            {
                result = number.TrimStart('+').TrimStart('0');
                result = result.Replace("(", "");
                result = result.Replace(")", "");
                result = result.Replace("-", "");
            }
            catch { }

            return result;
        }

        private string ShowHelp()
        {
            string info = "XPhone Connect Call Routing API" + "\r\n" + "\r\n";

            string help =
                  @"GET /clip" + "\r\n"
                + @"    Show help." + "\r\n"
                + @"GET /clip/license" + "\r\n"
                + @"    Show license info." + "\r\n"
                + @"POST /clip/store" + "\r\n"
                + @"    Store routing information for given agent." + "\r\n"
                + @"POST /clip/route" + "\r\n"
                + @"    Retrieve (modified) routing information for agent." + "\r\n"
                + @"POST /clip/read" + "\r\n"
                + @"    Like /clip/route, but leave all stored information unchanged." + "\r\n"
                + @"DELETE /clip/agents/{agent}" + "\r\n"
                + @"    Reset clip settings for {agent}" + "\r\n"
                + @"DELETE /clip/agents/{agent}/{option}" + "\r\n"
                + @"    Reset special 'clip' or 'cliponce' for {agent}. {option} = [clip | cliponce]" + "\r\n"
                ;

            string helpDeprecated =
                  @"DEPRECATED API:" + "\r\n"
                + @"/clip/{cmd}[?{request params}]" + "\r\n"
                + @"{cmd} store, find, peek, read, filter, reset, options, anynode/route" + "\r\n"
                + @"   store?agent={agent-number}&partner={partner-number}[&clip={clip-number}]" + "\r\n"
                + @"   find?agent={agent-number}&partner={partner-number}" + "\r\n"
                + @"   peek?agent={agent-number}&partner={partner-number}" + "\r\n"
                + @"   reset?agent={agent-number}" + "\r\n"
                + @"   filter?whitelist=[Enable/Disable/number]&blacklist=[Enable/Disable/number]" + "\r\n"
                + @"   options?agent={agent-number}[&clip={clip-number}][&cliponce={cliponce-number}]"
                ;

            return info + help + "\r\n" + helpDeprecated; ;
        }

        private bool IsValidLicense()
        {
            return ControllerLicense.valid;
        }
        #endregion

        // POST /clip/store
        [HttpPost("store")]
        public JsonResult Store([FromBody] object value)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();

            ClipResponse response = new ClipResponse();

            if (!IsValidLicense())
            {
                string msgErr = string.Format("ERR remoteIP='{0}' /clip/store: Invalid XPhone REST API license found.", client);
                logFile.Append(msgErr, true);

                return new JsonResult(response);
            }

            ClipRequest request = JsonSerializer.Deserialize<ClipRequest>(value.ToString());

            string agent = Normalize(request?.source?.number);
            string partner = Normalize(request?.destination?.number);
            string clip = Normalize(request?.value?.clip);
            string cliponce = Normalize(request?.value?.cliponce);

            if (!string.IsNullOrEmpty(agent))
            {
                IniFile ini = new IniFile(ClipControlFileName);
                if (!string.IsNullOrEmpty(clip))
                {
                    if (!string.IsNullOrEmpty(partner))
                    {
                        if (ini.IsValid(clip))
                        {
                            ini.WriteString(agent, partner, clip);
                            response.result = "0";
                        }
                    }
                    else
                    {
                        ini.WriteString(agent, "Clip", clip);
                        response.result = "0";
                    }
                }
                if (!string.IsNullOrEmpty(cliponce))
                {
                    ini.WriteString(agent, "ClipOnce", cliponce);
                    response.result = "0";
                }
            }

            return new JsonResult(response);
        }

        // POST /clip/route
        [HttpPost("route")]
        public JsonResult Route([FromBody] object value)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();

            ClipResponse response = new ClipResponse();

            if (!IsValidLicense())
            {
                string msgErr = string.Format("ERR remoteIP='{0}' /clip/route: Invalid XPhone REST API license found.", client);
                logFile.Append(msgErr, true);

                return new JsonResult(response);
            }

            ClipRequest request = JsonSerializer.Deserialize<ClipRequest>(value.ToString());

            string agent = Normalize(request?.source?.number);
            string partner = Normalize(request?.destination?.number);
            string newclip = null;

            if (!string.IsNullOrEmpty(agent) && !string.IsNullOrEmpty(partner))
            {
                newclip = GetClip(agent, partner);
                if (!String.IsNullOrWhiteSpace(newclip))
                {
                    response.destination = new ClipDestination();
                    response.destination.number = newclip;
                    response.result = "0";
                }
            }

            return new JsonResult(response);
        }

        // POST /clip/read
        [HttpPost("read")]
        public JsonResult Read([FromBody] object value)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();

            ClipResponse response = new ClipResponse();

            if (!IsValidLicense())
            {
                string msgErr = string.Format("ERR remoteIP='{0}' /clip/read: Invalid XPhone REST API license found.", client);
                logFile.Append(msgErr, true);

                return new JsonResult(response);
            }

            try
            {
                ClipRequest request = JsonSerializer.Deserialize<ClipRequest>(value.ToString());

                string agent = Normalize(request?.source?.number);
                string partner = Normalize(request?.destination?.number);
                string newclip = null;

                if (!string.IsNullOrEmpty(agent))
                {
                    newclip = GetClip(agent, partner, false);
                    if (!String.IsNullOrWhiteSpace(newclip))
                    {
                        response.source = new ClipSource();
                        response.source.number = request.source?.number;
                        response.source.name = request.source?.name;

                        response.destination = new ClipDestination();
                        response.destination.number = newclip;

                        response.value = new ClipValue();
                        IniFile ini = new IniFile(ClipControlFileName);
                        response.value.clip =  ini.ReadString(agent, "Clip");
                        response.value.cliponce = ini.ReadString(agent, "ClipOnce");

                        response.result = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                response.result = ex.HResult.ToString();
                response.reason = ex.Message;
            }

            return new JsonResult(response);
        }

        // DELETE /clip/agents/{agent}
        // DELETE /clip/agents/{agent}/{option}
        [HttpDelete("agents/{agent}")]
        [HttpDelete("agents/{agent}/{option}")]
        public JsonResult Delete(string agent, string option)
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();

            ClipResponse response = new ClipResponse();

            if (!IsValidLicense())
            {
                string msgErr = string.Format("ERR remoteIP='{0}' /clip/delete/{agent}: Invalid XPhone REST API license found.", client);
                logFile.Append(msgErr, true);

                return new JsonResult(response);
            }

            agent = Normalize(agent);
            if (!string.IsNullOrEmpty(agent))
            {
                IniFile ini = new IniFile(ClipControlFileName);
                if ( !string.IsNullOrEmpty(option) )
                {
                    if ( option.ToLower() == "clip" )
                    {
                        ini.WriteString(agent, "Clip", null);
                        response.result = "0";
                    }
                    if (option.ToLower() == "cliponce")
                    {
                        ini.WriteString(agent, "ClipOnce", null);
                        response.result = "0";
                    }
                }
                else
                {
                    ini.WriteString(agent, null, null);
                    response.result = "0";
                }
            }

            return new JsonResult(response);
        }

        // GET /clip/list
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public string List()
        {
            string txt = "Empty file.";
            try
            {
                txt = System.IO.File.ReadAllText(ClipControlFileName);
            }
            catch { }
            return txt;
        }

    }

    public class ClipSource
    {
        public string number { get; set; }
        public string name { get; set; }
        public string mail { get; set; }
    }

    public class ClipDestination
    {
        public string number { get; set; }
        public string name { get; set; }
        public string mail { get; set; }
    }

    public class ClipValue
    {
        public string clip { get; set; }
        public string cliponce { get; set; }
    }

    public class ClipRequest
    {
        public ClipSource source { get; set; }
        public ClipDestination destination { get; set; }
        public ClipValue value { get; set; }
    }

    public class ClipResponse
    {
        public ClipResponse()
        {
            result = "-1";
        }
        public string result { get; set; }
        public string reason { get; set; }

        public ClipSource source { get; set; }
        public ClipDestination destination { get; set; }
        public ClipValue value { get; set; }
    }
}

#endif //CLIP_CONTROLLER
