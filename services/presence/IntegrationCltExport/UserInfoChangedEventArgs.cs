using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace C4B.Atlas.Integration
{
    [DataContract]
    public class UserInfoChangedEventArgs : EventArgs, IExtensibleDataObject
    {
        public const int PresenceStateChanged = 0;
        public const int PresenceTextChanged = 1;
        public const int TelephoneStateChanged = 2;
        public const int TeamDeskAgentStateChanged = 3;

        public UserInfoChangedEventArgs(string a_userEmail, int a_changedInfo, object a_contentOld, object a_contentNew, string a_origin)
        {
            UserEmail = a_userEmail;
            ChangedInfo = a_changedInfo;
            ContentOld = a_contentOld;
            ContentNew = a_contentNew;
            Origin = a_origin;
        }

        [DataMember]
        public string UserEmail { get; private set; }
        [DataMember]
        public int ChangedInfo { get; private set; }
        [DataMember]
        public object ContentOld { get; private set; }
        [DataMember]
        public object ContentNew { get; private set; }
        [DataMember]
        public string Origin { get; private set; }

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
