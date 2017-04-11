using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Gateway.Handlers
{
    // TODO: Test Transient retries
    public class GatewayHandler : DelegatingHandler
    {
        // TODO: Need to do some testing to properly define the policy, current is based on link below.  This is more an example
        // https://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.getresponse(v=vs.110).aspx

        // TODO: List below maps to sample from Polly docs - however, this seems fairly complete based on
        // http://www.restapitutorial.com/httpstatuscodes.html
        private static readonly HttpStatusCode[] HttpStatusCodesWorthRetrying =
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout
        };

        private readonly HttpClient client;
        private readonly IServiceInstanceLookup instanceLookup;

        public GatewayHandler(HttpClient client, IServiceInstanceLookup instanceLookup)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (instanceLookup == null)
            {
                throw new ArgumentNullException(nameof(instanceLookup));
            }

            this.client = client;
            this.instanceLookup = instanceLookup;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var fabricAddress = request.RequestUri.ToFabricAddress();

            if (fabricAddress == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (await instanceLookup.GetAddress(fabricAddress, cancellationToken) == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            // TODO: Make the retry policy configurable
            var policy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => HttpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(0.2),
                    TimeSpan.FromSeconds(0.4),
                    TimeSpan.FromSeconds(0.8)
                });

            var response = await policy.ExecuteAsync(() => CallService(fabricAddress, request, cancellationToken));

            return response;
        }

        private async Task<HttpResponseMessage> CallService(string fabricAddress, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var serviceUri = await GetAddress(fabricAddress, request.RequestUri, cancellationToken);
            return await ProxyRequest(serviceUri, request);
        }

        private Task<HttpResponseMessage> ProxyRequest(Uri serviceUri, HttpRequestMessage request)
        {
            var proxiedRequest = request.Clone(serviceUri);
            
            return client.SendAsync(proxiedRequest);
        }

        private async Task<Uri> GetAddress(string fabricAddress, Uri requestUri, CancellationToken cancellationToken)
        {
            var baseUri = await instanceLookup.GetAddress(fabricAddress, cancellationToken);
            return new Uri(baseUri, requestUri.PathAndQuery);
        }
    }
}
