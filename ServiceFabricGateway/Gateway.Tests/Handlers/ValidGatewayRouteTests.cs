using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Gateway.Tests.Handlers
{
    [TestFixture]
    public class ValidGatewayRouteTests
    {
        [Test]
        [TestCase("finance/test")]
        [TestCase("Finance/Test")]
        [TestCase("FINANCE/TEST")]
        public async Task when_using_a_valid_route_the_response_is_proxied_regardless_of_case(string relativePath)
        {
            var request = new HttpRequestMessage {Method = HttpMethod.Get};

            var response = await new Specification()
                .WithRequestHandler(r => new HttpResponseMessage(HttpStatusCode.OK))
                .WithServiceRouting(f => f == "fabric:/finance-test/test" ? new Uri("http://sometestserver/") : null)
                .Send(request, relativePath);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        [TestCase("finance/test/customers")]
        [TestCase("finance/test/customers/10")]
        [TestCase("finance/test/customers/10/orders")]
        public async Task when_using_a_valid_route_with_additional_segments_the_response_is_proxied(string relativePath)
        {
            var request = new HttpRequestMessage { Method = HttpMethod.Get };

            var response = await new Specification()
                .WithRequestHandler(r => new HttpResponseMessage(HttpStatusCode.OK))
                .WithServiceRouting(f => f == "fabric:/finance-test/test" ? new Uri("http://sometestserver/") : null)
                .Send(request, relativePath);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        [TestCase("value=1")]
        [TestCase("name=Bill")]
        [TestCase("name=Bill&value=1")]
        public async Task when_using_a_query_string_the_request_is_proxied(string query)
        {
            var request = new HttpRequestMessage { Method = HttpMethod.Get};
            string recievedQuery = null;

            var response = await new Specification()
                .WithRequestHandler(r =>
                {
                    if (string.IsNullOrWhiteSpace(r.RequestUri.Query))
                    {
                        return new HttpResponseMessage(HttpStatusCode.NotFound);
                    }

                    recievedQuery = r.RequestUri.Query;
                    return new HttpResponseMessage(HttpStatusCode.OK);
                })
                .WithServiceRouting(f => new Uri("http://sometestserver"))
                .Send(request, "/finance/test?" + query);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(recievedQuery, Is.EqualTo("?" + query));
        }
    }
}