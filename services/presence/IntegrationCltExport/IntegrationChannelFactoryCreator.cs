using System;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace C4B.Atlas.Integration
{
    public class IntegrationChannelFactoryCreator<T> where T : IIntegrationClientBaseContract
    {
        public const String DefaultServerCertificateName = "C4B UCServer";
        public const int DefaultPort = 3344;

        private readonly string m_serverAddress;

        protected IntegrationChannelFactoryCreator(string a_serverAddress = "")
        {
            m_serverAddress = a_serverAddress;
        }

        public static ChannelFactory<T> Create()
        {
            bool isSecured;
            return Create(out isSecured);
        }

        public static ChannelFactory<T> Create(out bool a_isSecured)
        {
            return Create(string.Empty, out a_isSecured);
        }

        public static ChannelFactory<T> Create(string a_severAddress)
        {
            bool isSecured;
            return Create(a_severAddress, out isSecured);
        }

        public static ChannelFactory<T> Create(string a_serverAddress, out bool a_isSecured)
        {
            var creator = new IntegrationChannelFactoryCreator<T>(a_serverAddress);

            return creator.CreateCorrectChannel(out a_isSecured);
        }

        protected ChannelFactory<T> CreateCorrectChannel(out bool a_isSecured)
        {
            a_isSecured = false;
            ChannelFactory<T> channelFactoryDefault = null;
            ChannelFactory<T> channelFactorySecure = null;
            ChannelFactory<T> channelFactoryDefaultUnsecure = null;
            ChannelFactory<T> channelFactoryResult = null;

            try
            {
                // Binding 1 testen
                channelFactorySecure = CreateSecure();
                var serverVersion = GetServerVersion(channelFactorySecure);
                if (serverVersion >= 0)
                {
                    channelFactoryResult = channelFactorySecure;
                    channelFactorySecure = null;                    
                }

                // Binding 2 testen
                if (channelFactoryResult == null)
                {
                    channelFactoryDefault = CreateDefault();
                    serverVersion = GetServerVersion(channelFactoryDefault);
                    if (serverVersion >= 0)
                    {
                        channelFactoryResult = channelFactoryDefault;
                        channelFactoryDefault = null;
                    }                   
                }

                // Binding 3 testen
                if (channelFactoryResult == null)
                {
                    channelFactoryDefaultUnsecure = CreateDefaultUnsecure();
                    serverVersion = GetServerVersion(channelFactoryDefaultUnsecure);
                    if (serverVersion >= 0)
                    {
                        channelFactoryResult = channelFactoryDefaultUnsecure;
                        channelFactoryDefaultUnsecure = null;
                    }
                }
            }
            finally
            {
                SafeClose(channelFactorySecure, channelFactoryDefault, channelFactoryDefaultUnsecure);
            }

            if (channelFactoryResult == null)
                throw new Exception("Server is not available at the moment. Please try again later.");

            a_isSecured = IsSecure(channelFactoryResult);

            return channelFactoryResult;
        }

        private bool IsSecure(ChannelFactory<T> a_channelFactory)
        {
            if (a_channelFactory == null)
                return false;

            var binding = a_channelFactory.Endpoint.Binding;

            var tcpBinding = binding as NetTcpBinding;
            if (tcpBinding != null)
                return (tcpBinding.Security.Mode != SecurityMode.None);

            return false;
        }

        private ChannelFactory<T> CreateSecure()
        {
            var channelFactory = CreateFactory("C4B.Atlas.Integration.IntegrationClientServiceSecure");

            if (channelFactory.Credentials != null)
            {
                channelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode =
                    X509CertificateValidationMode.None;
            }

            SetIdentity(channelFactory);

            return channelFactory;
        }

        private ChannelFactory<T> CreateDefault()
        {
            var channelFactory = CreateFactory("C4B.Atlas.Integration.IntegrationClientServiceDefault");

            return channelFactory;
        }

        private ChannelFactory<T> CreateDefaultUnsecure()
        {
            var channelFactory = CreateFactory("C4B.Atlas.Integration.IntegrationClientServiceDefaultUnsecure");

            return channelFactory;
        }

        protected virtual ChannelFactory<T> CreateFactory(string a_configurationName)
        {
            var channelFactory = new ChannelFactory<T>(a_configurationName);

            SetAddress(channelFactory);

            return channelFactory;
        }

        private int GetServerVersion(ChannelFactory<T> a_channelFactory)
        {
            try
            {
                if (a_channelFactory != null)
                {
                    var proxy = a_channelFactory.CreateChannel();
                    var version = proxy.GetServerContractVersion();
                    return version;
                }
            }
            catch (Exception)
            {
                // ignore
            }
            return -1;
        }

        protected void SetAddress(ChannelFactory<T> a_channelFactory)
        {
            if (m_serverAddress != null)
            {
                var identity = a_channelFactory.Endpoint.Address.Identity;
                var uri = CreateUri(m_serverAddress, a_channelFactory.Endpoint.Address.Uri);
                var headers = a_channelFactory.Endpoint.Address.Headers;

                a_channelFactory.Endpoint.Address = new EndpointAddress(uri, identity, headers);
            }
        }

        private static Uri CreateUri(string a_serverName, Uri a_baseUri)
        {
            if (!String.IsNullOrWhiteSpace(a_serverName))
            {
                var uriBuilder = new UriBuilder("net.tcp://", a_serverName);
                if (uriBuilder.Port < 0)
                    uriBuilder.Port = DefaultPort;

                uriBuilder.Path = a_baseUri.AbsolutePath;

                return uriBuilder.Uri;
            }

            return a_baseUri;
        }

        private void SetIdentity(ChannelFactory<T> a_channelFactory)
        {
            if (a_channelFactory.Endpoint.Address.Identity == null)
            {
                var identity = EndpointIdentity.CreateDnsIdentity(DefaultServerCertificateName);
                var uri = a_channelFactory.Endpoint.Address.Uri;
                var headers = a_channelFactory.Endpoint.Address.Headers;

                a_channelFactory.Endpoint.Address = new EndpointAddress(uri, identity, headers);
            }
        }

        private void SafeClose(params ICommunicationObject[] a_communicationObjects)
        {
            foreach (var co in a_communicationObjects)
            {
                SafeClose(co);
            }
        }

        private void SafeClose(ICommunicationObject a_communicationObject)
        {
            try
            {
                if (a_communicationObject != null)
                {
                    a_communicationObject.BeginClose(TimeSpan.FromSeconds(5), a_ar => { }, null);
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
