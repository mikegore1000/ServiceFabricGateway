using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Tests.Handlers
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> messageCreator;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> messageCreator)
        {
            this.messageCreator = messageCreator;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.messageCreator(request));
        }
    }
}