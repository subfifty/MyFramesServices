using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace C4B.Atlas.Integration
{
    public interface IIntegrationClientEvents
    {
        [OperationContract(IsOneWay = true)]
        void PresenceChanged(List<PresenceChangedEventArgs> a_args);

        [OperationContract(IsOneWay = true)]
        void UserInfoChanged(List<UserInfoChangedEventArgs> a_args);
    }
}
