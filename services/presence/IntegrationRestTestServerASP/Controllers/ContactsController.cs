using C4B.Atlas.Integration;
using IntegrationRESTTestServerASP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;

namespace IntegrationRESTTestServerASP.Controllers
{
    [RoutePrefix("api/contacts")]
    public class ContactsController : ApiController
    {
        //private IPresenceService m_presenceService;

        //public ContactsController(IPresenceService a_presenceService)
        //{
        //    m_presenceService = a_presenceService;
        //}

        ///// <summary>
        ///// Retrieves a list of all users and their current presence information.
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("")]
        //public IEnumerable<PresenceMapEntry> List()
        //{
        //    return m_presenceService.GetPresenceList();
        //}

        /// <summary>
        /// Retrieves sample contact data
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("sample")]
        [ResponseType(typeof(PresenceMapEntry))]
        public IHttpActionResult GetSample()
        {
            List<Contact> contacts = new List<Contact>();
            contacts.Add(new Contact()
            {
                givenname = "Dirk",
                lastname = "Walkowiak",
                email = "dirk.walkowiak@c4b.de",
                id = "wal",
                imaddress = "dirk.walkowiak@c4bag.onmicrosoft.com",
                phonebusiness = "+49(89)840798-185",
                phonehome = "",
                phonemobile = ""
            });
            contacts.Add(new Contact()
            {
                givenname = "Alex",
                lastname = "Fuchs",
                email = "alexander.fuchs@c4b.de",
                id = "fua",
                imaddress = "alexander.fuchs@c4bag.onmicrosoft.com",
                phonebusiness = "+49(89)840798-237",
                phonehome = "",
                phonemobile = ""
            });
            
            //return new JsonResult<List<Contact>>(contacts, new Newtonsoft.Json.JsonSerializerSettings(), System.Text.Encoding.Default, this );
            return Json<List<Contact>>(contacts);
        }

    }

    public class Contact
    {
        public string givenname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string imaddress { get; set; }
        public string phonebusiness { get; set; }
        public string phonemobile { get; set; }
        public string phonehome { get; set; }
        public string id { get; set; }
    }

}