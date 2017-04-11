using System;
using System.Net.Http;
using Gateway.Handlers;
using NUnit.Framework;

namespace Gateway.Tests.Handlers
{
    [TestFixture]
    public class GatewayHandlerTests
    {
        [Test]
        public void given_a_null_client_an_exception_is_thrown()
        {
            Assert.Throws<ArgumentNullException>(() => new GatewayHandler(null, new FakeServiceInstanceLookup(null)));
        }

        [Test]
        public void given_a_null_instance_lookup_an_exception_is_thrown()
        {
            Assert.Throws<ArgumentNullException>(() => new GatewayHandler(new HttpClient(), null));
        }
    }
}
