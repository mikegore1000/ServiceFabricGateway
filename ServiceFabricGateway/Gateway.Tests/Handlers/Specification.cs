using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;

namespace Gateway.Tests.Handlers
{
    public class Specification
    {
        private Func<HttpRequestMessage, HttpResponseMessage> requestHandler;
        private Func<string, Uri> serviceRouting;

        public Specification WithRequestHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            this.requestHandler = handler;
            return this;
        }

        public Specification WithServiceRouting(Func<string, Uri> routing)
        {
            this.serviceRouting = routing;
            return this;
        }

        public Task<HttpResponseMessage> Send(HttpRequestMessage request, string relativePath)
        {
            var server = TestServer.Create(app => new StartupBootstrapper().Configuration(app, this.requestHandler, this.serviceRouting));

            request.RequestUri = new Uri(server.BaseAddress, relativePath);
            return server.HttpClient.SendAsync(request);
        }
    }
}