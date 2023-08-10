using System;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

#if ANYBELL_CONTROLLER

namespace XPhoneRestApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize("anybell")]
    public partial class AnyBellController : XPhoneControllerBase
    {
        private static string ControllerName = "anybell";
        private static string m_agent = "AnyBellChannels";
        private static LicenseObject ControllerLicense = ApiLicense.Instance.ParseLicenseObject("AnyBell");

        // GET /anybell
        [HttpGet]
        [AllowAnonymous]
        public string GetHelp()
        {
            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' ShowHelp()", client), true);
            return ShowHelp();
        }

        // GET: /anybell/[cmd]
        [HttpGet("{cmd}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<string> Get(string cmd)
        {
            if (!IsValidLicense())
                return "License not valid.";

            string result = "OK: cmd = " + cmd;

            LogFile logFile = Logfiles.Find(ControllerName);

            string client = GetRemoteIPAddress().ToString();
            string channel = null;
            string callstate = null;
            string newCallstate = null;

            switch (cmd)
            {
                #region cmd=reset
                case "reset":
                    result = Delete();
                    break;
                #endregion

                #region cmd=dump            
                case "dump":
                    string txt = System.IO.File.ReadAllText(AnyBellControlFileName);
                    result = txt;
                    break;
                #endregion

                #region cmd=write
                case "write":
                    if (Request.Query.ContainsKey("channel"))
                    {
                        channel = Request.Query["channel"];
                    }
                    if (Request.Query.ContainsKey("callstate"))
                    {
                        callstate = Request.Query["callstate"];
                    }
                    else
                    {
                        callstate = "undefined";
                    }

                    if ( channel != null && callstate != null )
                    {
                        IniFile ini2 = new IniFile(AnyBellControlFileName);
                        ini2.WriteString(m_agent, channel, callstate);

                        // Trigger longpolling
                        SimpleLongPolling.Publish(channel, callstate);
                    }
                    break;
                #endregion

                #region cmd=read
                case "read":
                    if (Request.Query.ContainsKey("channel"))
                    {
                        channel = Request.Query["channel"];
                    }
                    if (channel != null)
                    {
                        newCallstate = GetCallState(m_agent, channel);
                    }
                    if (!String.IsNullOrWhiteSpace(newCallstate))
                    {
                        result = newCallstate;
                    }
                    else
                    {
                        // Start longpolling
                        result = await DoLongPolling(channel);

                        if (result != SimpleLongPolling.TIMEOUT )
                        {
                            if (channel != null)
                            {
                                newCallstate = GetCallState(m_agent, channel);
                            }
                            if (!String.IsNullOrWhiteSpace(newCallstate))
                            {
                                result = newCallstate;
                            }
                        }
                    }
                    break;
                #endregion
            }
            
            string msg = string.Format("INF remoteIP='{5}' cmd='{0}' From='{1}' To='{2}' clip='{3}' newCallstate='{4}'",
                cmd,
                m_agent != null ? m_agent : "null",
                channel != null ? channel : "null",
                callstate != null ? callstate : "null",
                newCallstate != null ? newCallstate : "null",
                client);
            logFile.Append(msg, true);

            return result;
        }

        private static string AnyBellControlFileName
        {
            get
            {
                string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"C4B\AnyBell\AnyBellControl.Ini");
                string dir = Path.GetDirectoryName(path);
                Directory.CreateDirectory(dir);
                return path;
            }
        }

        static string GetCallState(string a_AgentNumber, string a_Channel, bool a_DeleteClickOnce = false)
        {
            IniFile ini = new IniFile(AnyBellControlFileName);

            string callstate = ini.ReadString(a_AgentNumber, a_Channel);
            string callstate_timestamp = ini.ReadString(a_AgentNumber, a_Channel + "_timestamp");

            
            if (long.TryParse(callstate_timestamp, out long ticks) )
            {
                if (DateTime.UtcNow.Ticks - ticks > 5*TimeSpan.TicksPerSecond)
                    callstate = "";
            }

            if (String.IsNullOrEmpty(callstate))
            {
                // todo
            }

            if (a_DeleteClickOnce)
            {
                ini.WriteString(a_AgentNumber, a_Channel, null);
            }

            return callstate;
        }

        //private IPAddress GetRemoteIPAddress(bool allowForwarded = true)
        //{
        //    if (allowForwarded)
        //    {
        //        // if you are allowing these forward headers, please ensure you are restricting context.Connection.RemoteIpAddress
        //        // to cloud flare ips: https://www.cloudflare.com/ips/
        //        try
        //        {
        //            string header = (HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault());
        //            if (IPAddress.TryParse(header, out IPAddress ip))
        //            {
        //                return ip;
        //            }
        //        }
        //        catch { }
        //    }
        //    try
        //    {
        //        return HttpContext.Connection.RemoteIpAddress;
        //    }
        //    catch 
        //    {
        //        return IPAddress.Parse("0.0.0.0");
        //    }
        //}
        
        public async Task<string> DoLongPolling(string a_channel = "sample-channel")
        {
            SimpleLongPolling lp = new SimpleLongPolling(a_channel);
            var message = await lp.WaitAsync();
            return message != null ? message : SimpleLongPolling.TIMEOUT;
        }

        private string ShowHelp()
        {
            string info = "XPhone Connect AnyBell API" + "\r\n" + "\r\n";

            string help =
                  @"GET /anybell" + "\r\n"
                + @"    Show help." + "\r\n"
                + @"GET /anybell/license" + "\r\n"
                + @"    Show license info." + "\r\n"
                + @"GET /anybell/channels" + "\r\n"
                + @"    List active channels with most recent signal." + "\r\n"
                + @"GET /anybell/channels/[channel]" + "\r\n"
                + @"    Longpolling for next signal on named channel." + "\r\n"
                + @"PUT /anybell/channels/[channel]/[callstate]" + "\r\n"
                + @"    Set new callstate for named channel." + "\r\n"
                + @"POST /anybell" + "\r\n"
                + @"    Allow more complex manipulations." + "\r\n"
                + @"DELETE /anybell" + "\r\n"
                + @"    Reset all channels, i.e. delete all active signals." + "\r\n"
                ;

            string helpDeprecated =
                  @"DEPRECATED API:" + "\r\n"
                + @"/AnyBell/{cmd}[?{request params}]" + "\r\n"
                + @"{cmd} write, read, reset, dump" + "\r\n"
                + @"   write?channel={channel-id}&callstate={Ringing/Connected/Dropped}" + "\r\n"
                + @"   read?channel={channel-id}" + "\r\n"
                + @"   reset" + "\r\n"
                ;

            return info + help + "\r\n" + helpDeprecated; ;
        }
    }
}

#endif // ANYBELL_CONTROLLER
