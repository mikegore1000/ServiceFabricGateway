using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using Gateway.Handlers;
using Owin;
using Polly;

namespace Gateway
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
            {
                string certThumbprint;

                using (var cert2 = new X509Certificate2(certificate))
                {
                    certThumbprint = cert2.Thumbprint;
                }

                using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var matchedCerts = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, true);

                    if (matchedCerts.Count > 0)
                    {
                        return true;
                    }
                }

                return errors == SslPolicyErrors.None;
            };

            var client = CreateClient();

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.MessageHandlers.Add(new ProbeHandler());
            config.MessageHandlers.Add(new GatewayHandler(client, new NamingServiceInstanceLookup(), CreateRetryPolicy()));
            appBuilder.UseWebApi(config);
        }

        private static Policy<HttpResponseMessage> CreateRetryPolicy()
        {
            HttpStatusCode[] httpStatusCodesWorthRetrying =
            {
                HttpStatusCode.ServiceUnavailable
            };

            // TODO: Make the retry policy configurable
            var policy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .RetryAsync(3);

            return policy;
        }

        private static HttpClient CreateClient()
        {
            return new HttpClient();
        }
    }
}
