using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XPhoneRestApi
{
    public class AnyBellRequest
    {
        public string channel { get; set; }
        public string callstate { get; set; }
        public string presencestate { get; set; }
    }

    public class AnyBellResponse
    {
        public AnyBellResponse()
        {
            result = "failed";
        }
        public string result { get; set; }
        public string channel { get; set; }
        public string callstate { get; set; }
        public string presencestate { get; set; }
    }


    public class LicenseInfo
    {
        public LicenseInfo()
        {
            package = new LicensePackage();
            license = new LicenseObject();
            customer = new LicenseCustomer();
            partner = new LicensePartner();
        }
        public LicensePackage package { get; set; }
        public LicenseObject license { get; set; }
        public LicenseCustomer customer { get; set; }
        public LicensePartner partner { get; set; }
    }

    public class AnyBellHelpDeprecated
    {
        public string description { get { return "XPhone Connect AnyBell API V1 (deprecated!)"; } }
        public string syntax { get { return "GET /anybell/[write|read|reset|dump][?{request params}]"; } }
        public string cmd_write { get { return "write?channel={channel-id}&callstate=[Ringing|Connected|Dropped]"; } }
        public string cmd_read { get { return "read?channel={channel-id}"; } }
    }

    public class AnyBellHelp
    {
        public string description { get { return "XPhone Connect AnyBell API"; } }
        public string get_license { get { return "GET /anybell/license"; } }
        public string get_channels { get { return "GET /anybell/channels"; } }
        public string get_channel { get { return "GET /anybell/channels/[channel]"; } }
        public string put_channel { get { return "PUT /anybell/channels/[channel]/[callstate]"; } }
        public string post { get { return "POST /anybell, Body=[AnyBellRequest]"; } }
        public string delete { get { return "DELETE /anybell"; } }

        public AnyBellHelpDeprecated deprecated { get { return new AnyBellHelpDeprecated();  } }
    }

}
