#define XPCALL

using System;

namespace C4B.VDir.WebService.Models
{
#if XPCALL
    /// <summary>
    /// Model class for all Xphone Call attributes
    /// </summary>
    public class XpCall : IIndexedPropertyObject
    {
        public string CALLNO { get; set; }
        public string DISPLAYNAME { get; set; }
        public string CALLEDNO { get; set; }
        public string CALLEDEMAIL { get; set; }
        public string TIME { get; set; }
        public string DURATION { get; set; }
        public string STATE { get; set; }
        public string REDIRECTTYPE { get; set; }
        public string REDIRECTCALLNO { get; set; }
        public string REDIRECTDISPLAYNAME { get; set; }

        public XpCall()
        {
        }

        public string this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case "CALLNO": return this.CALLNO;
                    case "DISPLAYNAME": return this.DISPLAYNAME;
                    case "CALLEDNO": return this.CALLEDNO;
                    case "CALLEDEMAIL": return this.CALLEDEMAIL;
                    case "TIME": return this.TIME;
                    case "DURATION": return this.DURATION;
                    case "STATE": return this.STATE;
                    case "REDIRECTTYPE": return this.REDIRECTTYPE;
                    case "REDIRECTCALLNO": return this.REDIRECTCALLNO;
                    case "REDIRECTDISPLAYNAME": return this.REDIRECTDISPLAYNAME;
                    case "IsEmpty": return this.IsEmpty.ToString();
                    default: return null;
                }
            }
        }

        public static string GetURLPropertyName(string propertyName)
        {
            switch (propertyName)
            {
                case "CALLNO": return "CallNo";
                case "DISPLAYNAME": return "DisplayName";
                case "CALLEDNO": return "CalledNo";
                case "CALLEDEMAIL": return "CalledEmail";
                case "TIME": return "Time";
                case "DURATION": return "Duration";
                case "STATE": return "State";
                case "REDIRECTTYPE": return "RedirectType";
                case "REDIRECTCALLNO": return "RedirectCallNo";
                case "REDIRECTDISPLAYNAME": return "RedirectDisplayName";
                default: return null;
            }
        }

        public bool IsEmpty { 
            get 
            {
                return (String.IsNullOrEmpty(this.CALLNO) &&
                    String.IsNullOrEmpty(this.DISPLAYNAME) &&
                    String.IsNullOrEmpty(this.CALLEDNO) &&
                    String.IsNullOrEmpty(this.CALLEDEMAIL) &&
                    String.IsNullOrEmpty(this.TIME) &&
                    String.IsNullOrEmpty(this.DURATION) &&
                    String.IsNullOrEmpty(this.STATE) &&
                    String.IsNullOrEmpty(this.REDIRECTTYPE) &&
                    String.IsNullOrEmpty(this.REDIRECTCALLNO) &&
                    String.IsNullOrEmpty(this.REDIRECTDISPLAYNAME));
            }
        }
    }
#endif
}