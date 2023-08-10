using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace C4B.Atlas.Integration
{
    [DataContract]
    public class PresenceChangedEventArgs : EventArgs
    {
        public PresenceChangedEventArgs(string a_userEmail, PresenceType a_type, object a_contentOld, object a_contentNew, string a_origin)
        {
            UserEmail = a_userEmail;
            Type = a_type;
            ContentOld = a_contentOld;
            ContentNew = a_contentNew;
            Origin = a_origin;
        }
        
        public enum PresenceType
        {
            PresenceState,
            PresenceText,
            TelephoneState
        }
        [DataMember]
        public string UserEmail { get; private set; }
        [DataMember]
        public PresenceType Type { get; private set; }
        [DataMember]
        public object ContentOld { get; private set; }
        [DataMember]
        public object ContentNew { get; private set; }
        [DataMember]
        public string Origin { get; private set; }
    }
}
