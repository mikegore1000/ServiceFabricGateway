using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using Gateway.Handlers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Owin;
using Polly;

namespace Gateway
{
    public static class Startup
    {
        private const int DefaultRetries = 3;

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

            var client = CreateHttpClient();

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.MessageHandlers.Add(new ApplicationInsightsTelemetryHandler(CreateTelemetryClient()));
            config.MessageHandlers.Add(new ProbeHandler());
            config.MessageHandlers.Add(new GatewayHandler(client, new NamingServiceInstanceLookup(), CreateRetryPolicy()));
            appBuilder.UseWebApi(config);
        }

        private static Policy<HttpResponseMessage> CreateRetryPolicy()
        {
            var config = FabricRuntime.GetActivationContext().GetConfigurationPackageObject("Config");
            int retries;
                
            if(!int.TryParse(config.Settings.Sections["Retries"].Parameters["Attempts"].Value, out retries))
            {
                // Assume a default policy
                retries = DefaultRetries;
            }

            HttpStatusCode[] httpStatusCodesWorthRetrying =
            {
                HttpStatusCode.ServiceUnavailable
            };

            var policy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .RetryAsync(retries);

            return policy;
        }

        private static HttpClient CreateHttpClient()
        {
            return new HttpClient();
        }

        private static TelemetryClient CreateTelemetryClient()
        {
            var config = FabricRuntime.GetActivationContext().GetConfigurationPackageObject("Config");
            var telemetry = config.Settings.Sections["Telemetry"];
            var instrumentationKey = telemetry.Parameters["InstrumentationKey"].Value;
            bool disableTelemetry;

            bool.TryParse(telemetry.Parameters["DisableTelemetry"].Value, out disableTelemetry);

            TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
            TelemetryConfiguration.Active.DisableTelemetry = disableTelemetry;

            return new TelemetryClient(TelemetryConfiguration.Active);
        }
    }
}
