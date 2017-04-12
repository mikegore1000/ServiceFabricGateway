using System;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Handlers;

namespace Gateway.Tests.Handlers
{
    public class FakeServiceInstanceLookup : IServiceInstanceLookup
    {
        private readonly Func<FabricAddress, Uri> lookupFunc;

        public FakeServiceInstanceLookup(Func<FabricAddress, Uri> lookupFunc)
        {
            this.lookupFunc = lookupFunc;
        }

        public Task<Uri> GetAddress(FabricAddress fabricAddress, CancellationToken cancellationToken)
        {
            return Task.FromResult(lookupFunc(fabricAddress));
        }
    }
}