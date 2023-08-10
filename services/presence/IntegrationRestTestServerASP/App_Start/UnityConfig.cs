using IntegrationRESTTestServer;
using IntegrationRESTTestServerASP.Services;
using System.Web.Http;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.WebApi;

namespace IntegrationRESTTestServerASP
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            var connector = new WcfConnector();
            var pService = new PresenceService(connector);
            container.RegisterType<IPresenceService>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => pService));

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}