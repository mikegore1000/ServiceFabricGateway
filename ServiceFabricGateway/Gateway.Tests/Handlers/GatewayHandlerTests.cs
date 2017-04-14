using System;
using System.Net.Http;
using Gateway.Handlers;
using NUnit.Framework;
using Polly;

namespace Gateway.Tests.Handlers
{
    [TestFixture]
    public class GatewayHandlerTests
    {
        [Test]
        public void given_a_null_client_an_exception_is_thrown()
        {
            Assert.Throws<ArgumentNullException>(() => new GatewayHandler(null, new FakeServiceInstanceLookup(null), Policy.NoOpAsync<HttpResponseMessage>()));
        }

        [Test]
        public void given_a_null_instance_lookup_an_exception_is_thrown()
        {
            Assert.Throws<ArgumentNullException>(() => new GatewayHandler(new HttpClient(), null, Policy.NoOpAsync<HttpResponseMessage>()));
        }

        [Test]
        public void given_a_null_retry_policy_an_exception_is_thrown()
        {
            Assert.Throws<ArgumentNullException>(() => new GatewayHandler(new HttpClient(), new FakeServiceInstanceLookup(null), null));
        }
    }
}
