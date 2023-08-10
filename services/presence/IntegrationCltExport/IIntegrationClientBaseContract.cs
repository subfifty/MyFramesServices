using System.ServiceModel;

namespace C4B.Atlas.Integration
{
    [ServiceContract(Namespace = "http://C4B.XPhone.IntegrationServicesContract")]
    public interface IIntegrationClientBaseContract
    {
        // Alle Versionen:
        //================
        [OperationContract(Name = "IIntegrationClientContract.GetServerContractVersion")]
        int GetServerContractVersion();
    }
}
