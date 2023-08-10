using C4B.Atlas.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationRESTTestServerASP.Services
{
    public interface IPresenceService
    {
        List<PresenceMapEntry> GetPresenceList();
        PresenceMapEntry GetUserPresence(string a_email);
        bool TryGetUserPresence(string a_email, out PresenceMapEntry a_presence);
        void SetUserPresence(PresenceMapEntry a_presence);
        void SetTeamDeskAgentStatus(string a_email, TeamDeskAgentState a_AgentState);
    }
}
