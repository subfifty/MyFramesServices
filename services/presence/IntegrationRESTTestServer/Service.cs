using C4B.Atlas.Integration;
using IntegrationRESTCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationRESTTestServer
{
    internal class Service : IService
    {
        [return: MessageParameter(Name = "Data")]
        public List<PresenceMapEntry> GetPresenceList()
        {
            try
            {
                return Program.WcfConnector.Proxy.GetEmailPresenceMap();
            }
            catch (FaultException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                return ErrorFactory.GenerateError<List<PresenceMapEntry>>(ex);
            }
        }

        [return: MessageParameter(Name = "Data")]
        public PresenceMapEntry GetUserPresence(string email)
        {
            try
            {
                if (Program.WcfConnector.Proxy.TryGetUserPresence(email, out var presence))
                    return presence;
                else
                    return ErrorFactory.GenerateError<PresenceMapEntry>("Email not found or not distinct", HttpStatusCode.NotFound);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ErrorFactory.GenerateError<PresenceMapEntry>(ex);
            }
        }

        [return: MessageParameter(Name = "Data")]
        public void SetUserPresenceAttribute(string email, string attribute, string value)
        {
            try
            {
                if (Program.WcfConnector.Proxy.TryGetUserPresence(email, out var presence))
                {
                    switch (attribute)
                    {
                        case "PresenceStateGuid":
                            presence.PresenceStateGuid = value;
                            break;
                        case "Origin":
                            presence.Origin = value;
                            break;
                        case "PresenceStateTeamStatusText":
                            presence.PresenceStateTeamStatusText = value;
                            break;
                        case "TeamDeskAgentState":
                            if (int.TryParse(value, out var state))
                                presence.TeamDeskAgentState = (TeamDeskAgentState)state;
                            else
                                ErrorFactory.GenerateError<object>("Could not convert " + value + " to an int");
                            break;
                        case "TelephoneState":
                            if (int.TryParse(value, out var tState))
                                presence.TelephoneState = (TelephoneStateFlags)tState;
                            else
                                ErrorFactory.GenerateError<object>("Could not convert " + value + " to an int");
                            break;
                        default:
                            ErrorFactory.GenerateError<object>("Unknown attribute");
                            break;
                    }
                    Program.WcfConnector.Proxy.SetUserPresence(presence);
                }
                else
                    ErrorFactory.GenerateError<object>("Email not found or not distinct");
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorFactory.GenerateError<object>(ex);
            }
        }

        [return: MessageParameter(Name = "Data")]
        public void SetUserPresence(string email, PresenceMapEntry presence)
        {
            try
            {
                Program.WcfConnector.Proxy.SetUserPresence(presence);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorFactory.GenerateError<object>(ex);
            }
        }
    }
}
