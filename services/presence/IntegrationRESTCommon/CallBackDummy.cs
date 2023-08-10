using C4B.Atlas.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationRESTCommon
{
    internal class CallBackDummy : IIntegrationClientEvents
    {
        public void PresenceChanged(List<PresenceChangedEventArgs> a_args)
        {
            throw new NotImplementedException();
        }

        public void UserInfoChanged(List<UserInfoChangedEventArgs> a_args)
        {
            throw new NotImplementedException();
        }
    }
}
