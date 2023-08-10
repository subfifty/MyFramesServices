using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

#if ANYBELL_CONTROLLER

namespace XPhoneRestApi.Controllers
{
    public partial class AnyBellController : XPhoneControllerBase
    {
        private static bool MustDeleteStorage = true;
        public AnyBellController() 
        {
            try 
            {
                if (MustDeleteStorage)
                {
                    MustDeleteStorage = false;
                    this.Delete();
                }
            }
            catch
            {
            }
        }

        // GET /anybell/license
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

        // GET /anybell/channels
        //[JWTAccess()]
        //[Authorize]
        [HttpGet("channels")]
        public string Dump()
        {
            if (!IsValidLicense())
                return "License not valid.";

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' Dump()", client), true);

            string txt = System.IO.File.ReadAllText(AnyBellControlFileName);

            if (string.IsNullOrEmpty(txt))
                txt = "No active channels found.";

            return txt;
        }

        // GET /anybell/channels/[channel]
        [HttpGet("channels/{channel}")]
        public async Task<JsonResult> GetChannels(string channel)
        {
            if (!IsValidLicense())
                return new JsonResult(InvalidLicense);

            string result = String.IsNullOrWhiteSpace(channel) ? "failed" : "success";
            string callstate = GetCallState(m_agent, channel, true);  

            // Start longpolling
            if (String.IsNullOrWhiteSpace(callstate))
            {
                callstate = await DoLongPolling(channel);

                if (callstate != SimpleLongPolling.TIMEOUT)
                {
                    LogFile logFile = Logfiles.Find(ControllerName);
                    string client = GetRemoteIPAddress().ToString();
                    string cmd = "GET";
                    string msg = string.Format("INF remoteIP='{0}' cmd='{1}' channel='{2}' callstate='{3}' result='{4}'",
                        client,
                        cmd,
                        channel != null ? channel : "null",
                        callstate,
                        result);
                    logFile.Append(msg, true);
                }
            }

            AnyBellResponse response = new AnyBellResponse();
            response.channel = channel;
            response.callstate = callstate;
            response.result = result;

            return new JsonResult(response);
        }

        // POST /anybell
        [HttpPost]
        public JsonResult Post([FromBody] object value)
        {
            if (!IsValidLicense())
                return new JsonResult(InvalidLicense);

            AnyBellRequest request = JsonSerializer.Deserialize<AnyBellRequest>(value.ToString());

            string result = "failed";
            string channel = request.channel;
            string callstate = request.callstate != null ? request.callstate : "undefined";
            string presenceState = request.presencestate;

            if (channel != null)
            {
                IniFile ini = new IniFile(AnyBellControlFileName);
                ini.WriteString(m_agent, channel, callstate);
                ini.WriteString(m_agent, channel + "_timestamp", DateTime.UtcNow.Ticks.ToString());

                // Trigger longpolling
                SimpleLongPolling.Publish(channel, callstate);
                result = "success";
            }

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            string cmd = "POST";
            string msg = string.Format("INF remoteIP='{0}' cmd='{1}' channel='{2}' callstate='{3}' result='{4}'",
                client,
                cmd,
                channel != null ? channel : "null",
                callstate,
                result);
            logFile.Append(msg, true);

            AnyBellResponse response = new AnyBellResponse();
            response.channel = channel;
            response.callstate = callstate;
            response.result = result;

            return new JsonResult(response);
        }

        // PUT /anybell/channels/[channel]/[callstate]
        [HttpPut("channels/{a_channel}/{a_callstate}")]
        public JsonResult Put(string a_channel, string a_callstate)
        {
            if (!IsValidLicense())
                return new JsonResult(InvalidLicense);

            string result = "failed";
            string callState = a_callstate != null ? a_callstate : "undefined";

            if (a_channel != null)
            {
                if ( false == SimpleLongPolling.ChannelExists(a_channel) )
                {
                    IniFile ini = new IniFile(AnyBellControlFileName);
                    ini.WriteString(m_agent, a_channel, callState);
                    ini.WriteString(m_agent, a_channel + "_timestamp", DateTime.UtcNow.Ticks.ToString());
                }

                // Trigger longpolling
                SimpleLongPolling.Publish(a_channel, callState);
                result = "success";
            }

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            string cmd = "PUT";
            string msg = string.Format("INF remoteIP='{0}' cmd='{1}' channel='{2}' callstate='{3}' result='{4}'",
                client,
                cmd,
                a_channel != null ? a_channel : "null",
                a_callstate,
                result);
            logFile.Append(msg, true);

            AnyBellResponse response = new AnyBellResponse();
            response.channel = a_channel;
            response.callstate = callState;
            response.result = result;

            return new JsonResult(response);
        }

        // DELETE /anybell
        [HttpDelete]
        public string Delete()
        {
            if (!IsValidLicense())
                return "License not valid.";

            LogFile logFile = Logfiles.Find(ControllerName);
            string client = GetRemoteIPAddress().ToString();
            logFile.Append(string.Format("INF remoteIP='{0}' Delete()", client), true);

            System.IO.File.Delete(AnyBellControlFileName);
            IniFile ini = new IniFile(AnyBellControlFileName);
            ini.WriteString(m_agent, null, null);
            string txt = System.IO.File.ReadAllText(AnyBellControlFileName);

            return txt;
        }

        private AnyBellResponse InvalidLicense { get { return new AnyBellResponse(); } }

        private bool IsValidLicense()
        {
            return ControllerLicense.valid;
        }
    }
}

#endif