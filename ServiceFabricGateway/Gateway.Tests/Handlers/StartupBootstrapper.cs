using System;
using System.Net.Http;
using System.Web.Http;
using Gateway.Handlers;
using Owin;
using Polly;

namespace Gateway.Tests.Handlers
{
    public class StartupBootstrapper
    {
        public void Configuration(IAppBuilder appBuilder, Func<HttpRequestMessage, HttpResponseMessage> requestHandler, Func<FabricAddress, Uri> serviceRouting)
        {
            var client = new HttpClient(new FakeHttpMessageHandler(requestHandler));

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.MessageHandlers.Add(new ProbeHandler());
            config.MessageHandlers.Add(new GatewayHandler(new FakeClientProxy(client, serviceRouting), 0));

            appBuilder.UseWebApi(config);
        }
    }
}