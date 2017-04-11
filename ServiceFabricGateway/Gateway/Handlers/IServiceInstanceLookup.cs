using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Handlers
{
    public interface IServiceInstanceLookup
    {
        Task<Uri> GetAddress(string fabricAddress, CancellationToken cancellationToken);
    }
}