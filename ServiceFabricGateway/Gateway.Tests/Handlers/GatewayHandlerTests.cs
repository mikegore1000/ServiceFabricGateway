using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

    // Bootstrapping the real OWIN configuration - will customise as we add more tests

    public class FakeServiceInstanceLookup : IServiceInstanceLookup
    {
        private readonly Func<string, Uri> lookupFunc;

        public FakeServiceInstanceLookup(Func<string, Uri> lookupFunc)
        {
            this.lookupFunc = lookupFunc;
        }

        public Task<Uri> GetAddress(string fabricAddress, CancellationToken cancellationToken)
        {
            return Task.FromResult(lookupFunc(fabricAddress));
        }
    }
}
