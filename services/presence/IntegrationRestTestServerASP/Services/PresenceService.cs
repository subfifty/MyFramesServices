using C4B.Atlas.Integration;
using IntegrationRESTTestServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace IntegrationRESTTestServerASP.Services
{
    public class PresenceService : IPresenceService
    {
        private WcfConnector m_connector;

        public PresenceService(WcfConnector a_connector)
        {
            m_connector = a_connector;
        }

        public List<PresenceMapEntry> GetPresenceList()
        {
            EnsureConnected();
            return m_connector.Proxy.GetEmailPresenceMap();
        }

        public PresenceMapEntry GetUserPresence(string a_email)
        {
            EnsureConnected();
            if (m_connector.Proxy.TryGetUserPresence(a_email, out var presence))
                return presence;
            else
                throw new KeyNotFoundException(a_email);
        }

        public bool TryGetUserPresence(string a_email, out PresenceMapEntry a_presence)
        {
            EnsureConnected();
            return m_connector.Proxy.TryGetUserPresence(a_email, out a_presence);
        }

        public void SetUserPresence(PresenceMapEntry a_presence)
        {
            EnsureConnected();
            m_connector.Proxy.SetUserPresence(a_presence);
        }

        public void SetTeamDeskAgentStatus(string a_email, TeamDeskAgentState a_AgentState)
        {
            EnsureConnected();
            m_connector.Proxy.SetTeamDeskAgentStatus(a_email, a_AgentState);
        }

        private void EnsureConnected()
        {
            if(m_connector.State != CommunicationState.Opened)
            {
                m_connector.Connect();
            }
        }

    }
}