using System;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace C4B.Atlas.Integration
{
    public class IntegrationDuplexChannelFactoryCreator<T> : IntegrationChannelFactoryCreator<T> where T : IIntegrationClientBaseContract
    {
        protected object m_callbackHandler;

        public static ChannelFactory<T> CreateDuplex(out bool a_isSecured, object a_callbackHandler)
        {
            return CreateDuplex(string.Empty, a_callbackHandler, out a_isSecured);
        }

        public static ChannelFactory<T> CreateDuplex(string a_serverAddress, object a_callbackHandler, out bool a_isSecured)
        {
            var creator = new IntegrationDuplexChannelFactoryCreator<T>(a_callbackHandler, a_serverAddress);

            return creator.CreateCorrectChannel(out a_isSecured);
        }

        protected IntegrationDuplexChannelFactoryCreator(object a_callbackHandler, string a_serverAddress = "") : base(a_serverAddress)
        {
            m_callbackHandler = a_callbackHandler;
        }

        protected override ChannelFactory<T> CreateFactory(string a_configurationName)
        {
            var channelFactory = new DuplexChannelFactory<T>(
                new InstanceContext(m_callbackHandler),
                a_configurationName);

            SetAddress(channelFactory);

            return channelFactory;
        }

    }
}
