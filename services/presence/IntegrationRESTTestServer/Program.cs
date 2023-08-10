using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationRESTTestServer
{
    class Program
    {
        internal static WcfConnector WcfConnector { get; private set; } = new WcfConnector();

        static void Main(string[] args)
        {
            try
            {
                WcfConnector.Connect();

                WebServiceHost hostWeb = new WebServiceHost(typeof(Service));
                hostWeb.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");
                ServiceDebugBehavior stp = hostWeb.Description.Behaviors.Find<ServiceDebugBehavior>();
                stp.HttpHelpPageEnabled = false;
                hostWeb.Open();

                Console.WriteLine("XPhone Connect Presence Rest Test Server started @ " + DateTime.Now.ToString());
                Console.Read();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
