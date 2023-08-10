using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XPhoneRestApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AnyBellJSONController : ControllerBase
    {
        private string agent = "AnyBellChannels";
        private static LicenseObject ControllerLicense = ApiLicense.Instance.ParseLicenseObject("AnyBell");

        // GET: /<AnyBellJSONController>
        [HttpGet]
        public JsonResult Get()
        {
            //if (Request.Query.ContainsKey("license"))
            //{
            //    LicenseInfo license = new LicenseInfo();

            //    license.license = ControllerLicense;
            //    license.customer = ApiLicense.Instance.CustomerInfo;
            //    license.partner = ApiLicense.Instance.PartnerInfo;

            //    return new JsonResult(license);
            //}
            //if (Request.Query.ContainsKey("dump"))
            //{
            //    string txt = System.IO.File.ReadAllText(AnyBellControlFileName);

            //    return new JsonResult(txt);
            //}

            return new JsonResult(new AnyBellHelp());
        }

        // GET: /<AnyBellJSONController>/License
        [HttpGet("license")]
        public JsonResult GetLicense()
        {
            LicenseInfo license = new LicenseInfo();

            license.license = ControllerLicense;
            license.customer = ApiLicense.Instance.CustomerInfo;
            license.partner = ApiLicense.Instance.PartnerInfo;

            return new JsonResult(license);
        }

        // GET: /<AnyBellJSONController>/Dump
        [HttpGet("channels")]
        public string Dump()
        {
            string txt = System.IO.File.ReadAllText(AnyBellControlFileName);

            return txt;
        }

        // GET /<AnyBellJSONController>/<channel>
        [HttpGet("channels/{channel}")]
        public async Task<JsonResult> Get(string channel)
        {
            if (!ControllerLicense.valid)
                return new JsonResult(InvalidLicense);

            string result = String.IsNullOrWhiteSpace(channel) ? "failed" : "success";
            string callState = null; // GetCallState(agent, channel);

            // Start longpolling
            if ( String.IsNullOrWhiteSpace(callState) )
            {
                callState = await DoLongPolling(channel);

                if (callState != "TIMEOUT")
                {
                    callState = GetCallState(agent, channel);
                }
            }

            AnyBellResponse response = new AnyBellResponse();
            response.channel = channel;
            response.callstate = callState;
            response.result = result;

            return new JsonResult(response);
        }

        // POST /<AnyBellJSONController>
        [HttpPost]
        public JsonResult Post([FromBody] object value)
        {
            if (!ControllerLicense.valid)
                return new JsonResult(InvalidLicense);

            AnyBellRequest request = JsonSerializer.Deserialize<AnyBellRequest>(value.ToString());

            string result = "failed";
            string channel = request.channel;
            string callState = request.callstate != null ? request.callstate : "undefined";
            string presenceState = request.presencestate;

            if (channel != null)
            {
                IniFile ini = new IniFile(AnyBellControlFileName);
                ini.WriteString(agent, channel, callState);

                // Trigger longpolling
                SimpleLongPolling.Publish(channel, callState);
                result = "success";
            }

            AnyBellResponse response = new AnyBellResponse();
            response.channel = channel;
            response.callstate = callState;
            response.result = result;

            return new JsonResult(response);
        }

        // PUT /<AnyBellJSONController>/<channel>/<callstate>
        [HttpPut("channels/{channel}/{callstate}")]
        public JsonResult Put(string channel, string callstate)
        {
            if (!ControllerLicense.valid)
                return new JsonResult(InvalidLicense);

            string result = "failed";
            string callState = callstate != null ? callstate : "undefined";

            if (channel != null)
            {
                IniFile ini = new IniFile(AnyBellControlFileName);
                ini.WriteString(agent, channel, callState);

                // Trigger longpolling
                SimpleLongPolling.Publish(channel, callState);
                result = "success";
            }

            AnyBellResponse response = new AnyBellResponse();
            response.channel = channel;
            response.callstate = callState;
            response.result = result;

            return new JsonResult(response);
        }

        // DELETE /<AnyBellJSONController>
        [HttpDelete]
        public string Delete()
        {
            System.IO.File.Delete(AnyBellControlFileName);
            IniFile ini = new IniFile(AnyBellControlFileName);
            ini.WriteString(agent, null, null);
            string txt = System.IO.File.ReadAllText(AnyBellControlFileName);
            return txt;
        }

        public async Task<string> DoLongPolling(string a_channel = "sample-channel")
        {
            SimpleLongPolling lp = new SimpleLongPolling(a_channel);
            var message = await lp.WaitAsync();
            return message != null ? message : "TIMEOUT";
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

        private AnyBellResponse InvalidLicense { get { return new AnyBellResponse(); } }

        static string GetCallState(string a_AgentNumber, string a_Channel, bool a_DeleteClickOnce = true)
        {
            IniFile ini = new IniFile(AnyBellControlFileName);

            string callstate = ini.ReadString(a_AgentNumber, a_Channel);

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
    }
}
