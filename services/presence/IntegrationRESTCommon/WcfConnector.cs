using C4B.Atlas.Integration;
using IntegrationRESTCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Timers;

namespace IntegrationRESTTestServer
{
    public class WcfConnector
    {
        public IIntegrationClientContract_v2 Proxy { get; private set; }
        public CommunicationState State
        {
            get
            {
                try
                {
                    return ((IClientChannel)Proxy).State;
                }
                catch
                {
                    return CommunicationState.Closed;
                }
            }
        }

        private ChannelFactory<IIntegrationClientContract_v2> m_channelFactory;

        private Timer m_keepAliveTimer;
        private TimeSpan m_keepAliveInterval = new TimeSpan(0, 0, 30);

        public void Connect()
        {
            EnsureChannelFactory();
            EnsureProxy();
            StartKeepAlive();
        }

        private void StartKeepAlive()
        {
            m_keepAliveTimer = new Timer();
            m_keepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
            m_keepAliveTimer.Interval = (int)m_keepAliveInterval.TotalMilliseconds;
            m_keepAliveTimer.Start();
        }

        private void KeepAliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Proxy.KeepAlive();
            }
            catch
            {
                //Verbindung anderweitig unterbrochen, versuchen neu aufzubauen.
                try
                {
                    Console.WriteLine("Retrying connection");
                    EnsureProxy(true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        private void StopKeepAlive()
        {
            m_keepAliveTimer.Stop();
        }

        private void EnsureProxy(bool a_force = false)
        {
            if (Proxy != null && !a_force)
                return;

            EnsureChannelFactory(true);

            Proxy = m_channelFactory.CreateChannel();
            ((IClientChannel)Proxy).Open();
        }

        private void EnsureChannelFactory(bool a_force = false)
        {
            if (m_channelFactory == null || a_force)
                m_channelFactory = IntegrationDuplexChannelFactoryCreator<IIntegrationClientContract_v2>.CreateDuplex(out _, new CallBackDummy());
        }
    }
}
