using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Gateway.Tests.Handlers
{
    [TestFixture]
    public class InvalidGatewayRouteTests
    {
        [Test]
        public async Task when_using_a_route_that_does_not_match_return_a_404()
        {
            var request = new HttpRequestMessage {Method = HttpMethod.Get};
            
            var response = await new Specification()
                .WithServiceRouting(f => {
                    throw new System.Fabric.FabricServiceNotFoundException();
                })
                .Send(request, "invalid/route");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task when_using_a_route_with_insufficient_segments_return_a_404()
        {
            var request = new HttpRequestMessage {Method = HttpMethod.Get};

            var response = await new Specification()
                .Send(request, "invalid");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}