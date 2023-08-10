using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace C4B.Atlas.Integration
{
    [DataContract]
    public class PresenceMapEntry : IExtensibleDataObject
    {
        [DataMember]
        public string UserEmail { get; private set; }

        [DataMember]
        public string PresenceStateGuid { get; set; }

        [DataMember]
        public string PresenceStateTeamStatusText { get; set; }

        [DataMember]
        public TelephoneStateFlags TelephoneState { get; set; }

        [DataMember]
        public string Origin { get; set; }

        [DataMember]
        public TeamDeskAgentState TeamDeskAgentState { get; set; }

        public PresenceMapEntry(string a_userEmail)
        {
            UserEmail = a_userEmail;
        }

        public static PresenceMapEntry FromTuple(Tuple<string, string, string, int> a_tuple)
        {
            return new PresenceMapEntry(a_tuple.Item1)
            {
                PresenceStateGuid = a_tuple.Item2,
                PresenceStateTeamStatusText = a_tuple.Item3,
                TelephoneState = (TelephoneStateFlags)a_tuple.Item4,
            };
        }

        #region IExtensibleDataObject

        private ExtensionDataObject m_extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return m_extensionData;
            }
            set
            {
                m_extensionData = value;
            }
        }

        #endregion

    }
}
