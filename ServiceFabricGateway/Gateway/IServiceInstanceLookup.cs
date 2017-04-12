using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway
{
    public interface IServiceInstanceLookup
    {
        Task<Uri> GetAddress(FabricAddress fabricAddress, CancellationToken cancellationToken);
    }
}