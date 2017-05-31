using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Tests.Handlers
{
    // TODO: Determine what to do if we get the: FabricServiceNotFoundException

    public class FakeClientProxy : IClientProxy
    {
        private readonly HttpClient httpClient;
        private readonly Func<FabricAddress, Uri> lookupFunc;

        public FakeClientProxy(HttpClient httpClient, Func<FabricAddress, Uri> lookupFunc)
        {
            this.httpClient = httpClient;
            this.lookupFunc = lookupFunc;
        }

        public async Task<HttpResponseMessage> ProxyToService(FabricAddress fabricAddress, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = await LookupServiceAddress(fabricAddress, cancellationToken);
            var serviceUri = GetServiceUri(uri, request.RequestUri);

            return await ProxyRequest(serviceUri, request, httpClient);
        }

        private Task<HttpResponseMessage> ProxyRequest(Uri serviceUri, HttpRequestMessage request, HttpClient client)
        {
            var proxiedRequest = request.Clone(serviceUri);

            return client.SendAsync(proxiedRequest);
        }

        private Uri GetServiceUri(Uri baseUri, Uri requestUri)
        {
            return new Uri(baseUri, requestUri.PathAndQuery);
        }

        private Task<Uri> LookupServiceAddress(FabricAddress fabricAddress, CancellationToken cancellationToken)
        {
            return Task.FromResult(lookupFunc(fabricAddress));
        }
    }
}