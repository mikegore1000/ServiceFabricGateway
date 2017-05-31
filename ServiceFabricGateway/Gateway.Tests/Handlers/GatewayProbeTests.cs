using System.Net;
using System.Net.Http;
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
}