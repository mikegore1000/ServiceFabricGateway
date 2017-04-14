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
        private static readonly HttpStatusCode[] HttpStatusCodesWorthRetrying =
        {
            HttpStatusCode.ServiceUnavailable
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
            try
            {
                var fabricAddress = new FabricAddress(request.RequestUri);

                if (await instanceLookup.GetAddress(fabricAddress, cancellationToken) == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                // TODO: Make the retry policy configurable
                var policy = Policy
                    .Handle<HttpRequestException>()
                    .OrResult<HttpResponseMessage>(r => HttpStatusCodesWorthRetrying.Contains(r.StatusCode))
                    .RetryAsync(3);

                var response = await policy.ExecuteAsync(() => CallService(fabricAddress, request, cancellationToken));

                return response;
            }
            catch (FabricAddress.InvalidFabricAddressException)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        private async Task<HttpResponseMessage> CallService(FabricAddress fabricAddress, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var serviceUri = await GetAddress(fabricAddress, request.RequestUri, cancellationToken);
            return await ProxyRequest(serviceUri, request);
        }

        private Task<HttpResponseMessage> ProxyRequest(Uri serviceUri, HttpRequestMessage request)
        {
            var proxiedRequest = request.Clone(serviceUri);
            
            return client.SendAsync(proxiedRequest);
        }

        private async Task<Uri> GetAddress(FabricAddress fabricAddress, Uri requestUri, CancellationToken cancellationToken)
        {
            var baseUri = await instanceLookup.GetAddress(fabricAddress, cancellationToken);
            return new Uri(baseUri, requestUri.PathAndQuery);
        }
    }
}
