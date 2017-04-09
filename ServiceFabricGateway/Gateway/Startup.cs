using System.Net.Http;
using System.Web.Http;
using Gateway.Handlers;
using Owin;

namespace Gateway
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            var client = CreateClient();

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.MessageHandlers.Add(new GatewayHandler(client));

            appBuilder.UseWebApi(config);
        }

        private static HttpClient CreateClient()
        {
            // TODO: Take into account this http://byterot.blogspot.co.uk/2016/07/singleton-httpclient-dns.html

            return new HttpClient();
        }
    }
}
