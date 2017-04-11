using System;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Handlers;

namespace Gateway.Tests.Handlers
{
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