using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway
{
    public interface IClientProxy
    {
        Task<HttpResponseMessage> ProxyToService(FabricAddress fabricAddress, HttpRequestMessage request, CancellationToken cancellationToken);
    }
}