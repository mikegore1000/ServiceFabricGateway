using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Gateway
{
    public class HttpCommunicationClientFactory : CommunicationClientFactoryBase<HttpCommunicationClient>
    {
        private readonly HttpClient httpClient;

        public HttpCommunicationClientFactory(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        protected override bool ValidateClient(HttpCommunicationClient client)
        {
            return true;
        }

        protected override bool ValidateClient(string endpoint, HttpCommunicationClient client)
        {
            return true;
        }

        protected override Task<HttpCommunicationClient> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpCommunicationClient(new Uri(endpoint), httpClient));
        }

        protected override void AbortClient(HttpCommunicationClient client)
        {
        }
    }
}