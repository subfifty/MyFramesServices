using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace C4B.Atlas.Integration
{
    [ServiceContract(
        Namespace = "http://C4B.XPhone.IntegrationServicesContract",
        CallbackContract = typeof(IIntegrationClientEvents)
    )]
    public interface IIntegrationClientContract_v2 : IIntegrationClientBaseContract
    {
        [OperationContract]
        int GetPresenceInfo(string EmailAddress, out string StatusGuid, out string StatusText);

        [OperationContract]
        int ChangePresenceInfo(string EmailAddress, string StatusGuid, string StatusText = "", string Language = "de");

        // Version 4:
        //===========
        // Zusätzlich zu Version 3 kann der Telefonstatus abgefragt werden.
        // Events für Präsenzänderungen

        [OperationContract]
        List<PresenceMapEntry> GetEmailPresenceMap();

        [OperationContract]
        void SubscribeUserPresenceChangedEvent();

        [OperationContract]
        void UnSubscribeUserPresenceChangedEvent();

        [OperationContract]
        void KeepAlive();

        // Version 5:
        //===========
        // Präsenzstatus für einzelne Benutzer

        [OperationContract]
        bool TryGetUserPresence(string EmailAddress, out PresenceMapEntry PresenceData);

        [OperationContract]
        void RemovePresenceSetByOrigin(string EmailAddress, string Origin);

        [OperationContract]
        void SetUserPresence(PresenceMapEntry PresenceData, string Language = "de");

        // Version 6:
        //==========
        // Erweiterung um TeamDeskAgentState
        [OperationContract]
        void SubscribeUserInfoChangedEvent();

        [OperationContract]
        void UnSubscribeUserInfoChangedEvent();

        [OperationContract]
        bool SetTeamDeskAgentStatus(string EmailAddress, TeamDeskAgentState AgentState);
    }
}
