using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;

namespace Gateway.Handlers
{
    // TODO: Transient retries
    public class GatewayHandler : DelegatingHandler
    {
        private readonly HttpClient client;

        private readonly List<GatewayRoute> routes = new List<GatewayRoute>
        {
            new GatewayRoute("/finance/test", "fabric:/ServiceFabricSpiking/TestApi")
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

            var serviceUri = await GetAddress(matchedRoute, routePostfix, cancellationToken);
            var response = await ProxyRequest(serviceUri, request);

            return response;
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
