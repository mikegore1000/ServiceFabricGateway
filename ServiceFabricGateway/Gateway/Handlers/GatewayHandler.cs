using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;
using Polly;

namespace Gateway.Handlers
{
    // TODO: Transient retries
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

        private readonly List<GatewayRoute> routes = new List<GatewayRoute>
        {
            new GatewayRoute("/finance/test", "fabric:/ServiceFabricSpiking/TestApi"),
            new GatewayRoute("/finance/test2", "fabric:/ServiceFabricSpiking/TestApi2")
        };

        public GatewayHandler(HttpClient client)
        {
            this.client = client;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.Segments.Length < 3)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var routeKey = $"{request.RequestUri.Segments[0]}{request.RequestUri.Segments[1]}{request.RequestUri.Segments[2]}";
            var matchedRoute = routes.SingleOrDefault(r => r.Matches(routeKey));
            var routePostfix = string.Concat(request.RequestUri.Segments.Skip(3));

            if (matchedRoute == null)
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

            var response = await policy.ExecuteAsync(() => CallService(request, cancellationToken, matchedRoute, routePostfix));

            return response;
        }

        private async Task<HttpResponseMessage> CallService(HttpRequestMessage request, CancellationToken cancellationToken, GatewayRoute matchedRoute, string routePostfix)
        {
            var serviceUri = await GetAddress(matchedRoute, routePostfix, cancellationToken);
            return await ProxyRequest(serviceUri, request);
        }

        private Task<HttpResponseMessage> ProxyRequest(Uri serviceUri, HttpRequestMessage request)
        {
            var proxiedRequest = request.Clone(serviceUri);
            
            return client.SendAsync(proxiedRequest);
        }

        private async Task<Uri> GetAddress(GatewayRoute matchedRoute, string routePostfix, CancellationToken cancellationToken)
        {
            var resolver = ServicePartitionResolver.GetDefault();
            var resolved = await resolver.ResolveAsync(
                new Uri(matchedRoute.FabricAddress),
                new ServicePartitionKey(),
                cancellationToken
            );

            JObject addresses = JObject.Parse(resolved.GetEndpoint().Address);
            var baseUri = new Uri((string) addresses["Endpoints"].First());
            return new Uri(baseUri, $"{matchedRoute.RouteKey}/{routePostfix}");
        }
    }
}
