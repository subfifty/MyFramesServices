using C4B.Atlas.Integration;
using IntegrationRESTTestServerASP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace IntegrationRESTTestServerASP.Controllers
{
    [RoutePrefix("api/presence")]
    public class PresenceController : ApiController
    {
        private IPresenceService m_presenceService;

        public PresenceController(IPresenceService a_presenceService)
        {
            m_presenceService = a_presenceService;
        }

        /// <summary>
        /// Retrieves a list of all users and their current presence information.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IEnumerable<PresenceMapEntry> List()
        {
            return m_presenceService.GetPresenceList();
        }

        /// <summary>
        /// Retrieves presence information for a single user.
        /// </summary>
        /// <param name="a_email">The (distinct!) email address of the user.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{a_email}")]
        [ResponseType(typeof(PresenceMapEntry))]
        public IHttpActionResult GetByEmail(string a_email)
        {
            if (m_presenceService.TryGetUserPresence(a_email, out var presence))
                return Ok(presence);
            else
                return NotFound();
        }

        /// <summary>
        /// Sets the presence of a user to the one given in the request body.
        /// The user is identified by his email in the PresenceMapEntry object
        /// </summary>
        /// <param name="a_value">A (json/xml) encoded object of type PresenceMapEntry. See the output of the GET method for example objects.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Edit([FromBody] PresenceMapEntry a_value)
        {
            try
            {
                m_presenceService.SetUserPresence(a_value);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Changes a single attribute of a users' presence to a new value. If you want to edit multiple attributes at once, use POST /presence.
        /// </summary>
        /// <param name="a_email">The (distinct!) email address of the user.</param>
        /// <param name="a_attribute">The name of the attribute. E.G Origin, TeamDeskAgentState or PresenceStateGuid. For a full list, see the output of GET /presence</param>
        /// <param name="a_value">The new value to be set. URLEncode spaces and other special characters.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{a_email}/edit")]
        public IHttpActionResult Edit(string a_email, [FromUri(Name = "attribute")] string a_attribute, [FromUri(Name = "value")] string a_value)
        {
            if (m_presenceService.TryGetUserPresence(a_email, out var presence))
            {
                switch (a_attribute)
                {
                    case "PresenceStateGuid":
                        presence.PresenceStateGuid = a_value;
                        break;
                    case "Origin":
                        presence.Origin = a_value;
                        break;
                    case "PresenceStateTeamStatusText":
                        presence.PresenceStateTeamStatusText = a_value;
                        break;
                    case "TeamDeskAgentState":
                        if (int.TryParse(a_value, out var state))
                        {
                            m_presenceService.SetTeamDeskAgentStatus(a_email, (TeamDeskAgentState)state);
                            return Ok();
                        }
                        else
                        {
                            return BadRequest("Could not parse TeamDeskAgentState " + a_value);
                        }
                    case "TelephoneState":
                        if (int.TryParse(a_value, out var tState))
                            presence.TelephoneState = (TelephoneStateFlags)tState;
                        else
                            return BadRequest("Could not Parse Telephone State " + a_value);
                        break;
                    default:
                        return BadRequest("Unknown attribute");
                }
                m_presenceService.SetUserPresence(presence);
                return Ok();
            }
            else
                return NotFound();
        }
    }
}