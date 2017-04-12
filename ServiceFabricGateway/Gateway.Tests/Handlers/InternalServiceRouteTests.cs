using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Gateway.Tests.Handlers
{
    [TestFixture]
    public class InternalServiceRouteTests
    {
        [Test]
        public async Task when_proxying_then_the_request_is_rejected_and_not_proxied()
        {
            var request = new HttpRequestMessage { Method = HttpMethod.Get };

            var result = await new Specification()
                .WithRequestHandler(r => new HttpResponseMessage(HttpStatusCode.OK))
                .WithServiceRouting(f => new Uri("http://mytestserver"))
                .Send(request, "finance/test-internal");

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
