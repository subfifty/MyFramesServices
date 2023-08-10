using C4B.Atlas.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationRESTTestServer
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Json,
             BodyStyle = WebMessageBodyStyle.Wrapped,
             UriTemplate = "presence")]
        [return: MessageParameter(Name = "Data")]
        List<PresenceMapEntry> GetPresenceList();

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Json,
             BodyStyle = WebMessageBodyStyle.Wrapped,
             UriTemplate = "presence/{email}")]
        [return: MessageParameter(Name = "Data")]
        PresenceMapEntry GetUserPresence(string email);

        [OperationContract]
        [WebInvoke(Method = "POST",
             ResponseFormat = WebMessageFormat.Json,
             BodyStyle = WebMessageBodyStyle.Wrapped,
             UriTemplate = "presence/{email}")]
        [return: MessageParameter(Name = "Data")]
        void SetUserPresence(string email, PresenceMapEntry presence);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Json,
             BodyStyle = WebMessageBodyStyle.Wrapped,
             UriTemplate = "presence/{email}/edit?attribute={attribute}&value={value}")]
        [return: MessageParameter(Name = "Data")]
        void SetUserPresenceAttribute(string email, string attribute, string value);
    }
}
