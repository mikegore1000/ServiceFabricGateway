using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Gateway.Tests.Handlers
{
    [TestFixture]
    public class GatewayProbeTests
    {
        [Test]
        [TestCase("probe")]
        [TestCase("probe/")]
        public async Task when_requesting_the_probe_status_a_200_is_returned(string probeRelativePath)
        {
            var request = new HttpRequestMessage {Method = HttpMethod.Get};

            var response = await new Specification()
                .Send(request, probeRelativePath);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }

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
                .WithServiceRouting(f => f.Uri == new Uri("fabric:/finance-test/test") ? new Uri("http://sometestserver/") : null)
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
                .WithServiceRouting(f => f.Uri == new Uri("fabric:/finance-test/test") ? new Uri("http://sometestserver/") : null)
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

        [Test]
        public async Task when_sending_a_body_it_is_proxied_to_and_from()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent("TEST-BODY")
            };

            var result = await new Specification()
                .WithRequestHandler(r =>
                {
                    var body = r.Content.ReadAsStringAsync().Result;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(body)
                    };
                })
                .WithServiceRouting(x => new Uri("http://mytestserver"))
                .Send(request, "/finance/test");

            var resultBody = await result.Content.ReadAsStringAsync();

            Assert.That(resultBody, Is.EqualTo("TEST-BODY"));
        }

        [Test]
        public async Task when_sending_headers_they_are_proxied()
        {
            string capturedRequestHeaderValue = null;

            var request = new HttpRequestMessage { Method = HttpMethod.Get };
            request.Headers.Add("X-MyRequestHeader", "REQUEST");

            var result = await new Specification()
                .WithRequestHandler(r =>
                {
                    capturedRequestHeaderValue = r.Headers.GetValues("X-MyRequestHeader").SingleOrDefault();

                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Headers.Add("X-MyResponseHeader", "RESPONSE");
                    return response;
                })
                .WithServiceRouting(x => new Uri("http://mytestserver"))
                .Send(request, "/finance/test");
           
            Assert.That(capturedRequestHeaderValue, Is.EqualTo("REQUEST"));
            Assert.That(result.Headers.GetValues("X-MyResponseHeader").Single(), Is.EqualTo("RESPONSE"));
        }
    }
}