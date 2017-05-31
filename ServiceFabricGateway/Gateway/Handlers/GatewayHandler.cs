using System;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Gateway.Handlers
{
    public class GatewayHandler : DelegatingHandler
    {
        private readonly Policy<HttpResponseMessage> retryPolicy;
        private readonly IClientProxy clientProxy;

        private static readonly HttpStatusCode[] HttpStatusCodesWorthRetrying =
            {
                HttpStatusCode.ServiceUnavailable
            };

        public GatewayHandler(IClientProxy clientProxy, int retries)
        {
            if (clientProxy == null)
            {
                throw new ArgumentNullException(nameof(clientProxy));
            }

            if (retries < 0)
            {
                throw new ArgumentException("The number of retries must be greater than or equal to zero", nameof(retries));
            }

            this.clientProxy = clientProxy;
            this.retryPolicy = CreateRetryPolicy(retries);
        } 

        private Policy<HttpResponseMessage> CreateRetryPolicy(int retries)
        {
            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => HttpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .RetryAsync(retries);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var fabricAddress = new FabricAddress(request.RequestUri);

                return await retryPolicy.ExecuteAsync(() => clientProxy.ProxyToService(fabricAddress, request, cancellationToken));
            }
            catch (FabricAddress.InvalidFabricAddressException)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            catch (FabricServiceNotFoundException)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }
    }
}
