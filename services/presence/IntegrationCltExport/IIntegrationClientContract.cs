using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace C4B.Atlas.Integration
{
    [ServiceContract(
       Namespace = "http://C4B.XPhone.IntegrationServicesContract"
   )]
    public interface IIntegrationClientContract
    {
        // Alle Versionen:
        //================
        [OperationContract(Name = "IIntegrationClientContract.GetServerContractVersion")]
        int GetServerContractVersion();

        // Version 2:
        //===========
        [OperationContract(Name = "IIntegrationClientContract.GetEmailPresenceMap")]
        List<Tuple<string, string>> GetEmailPresenceMap();

        // Version 3:
        //===========

        // Info:
        // Über das API kann man die Zeitdauer nicht steuern. Das ist Absicht, damit beim Ablaufen der Zeitspanne keine ungewollten Dinge passieren. 
        // Konzept: es wird immer die aktuelle Zeitspanne des aktuellen Termins weiter verwendet. Damit kann man praktisch nichts falsch machen!
        // Umleitungen können über das API auch nicht geändert werden. Auch Absicht. 
        // Den StatusText muss man sich vom Server beschaffen können, damit man z.B. zwischen "Anwesend" und "Anwesend im Homeoffice" unterscheiden kann. 
        // Daher die Erweiterung in GetEmailPresenceMap3().

        [OperationContract(Name = "IIntegrationClientContract.GetEmailPresenceMap3")]
        List<Tuple<string, string, string>> GetEmailPresenceMap3();

        [OperationContract(Name = "IIntegrationClientContract.GetPresenceInfo")]
        int GetPresenceInfo(string EmailAddress, out string StatusGuid, out string StatusText);

        [OperationContract(Name = "IIntegrationClientContract.ChangePresenceInfo")]
        int ChangePresenceInfo(string EmailAddress, string StatusGuid, string StatusText = "", string Language = "de");
    }
}