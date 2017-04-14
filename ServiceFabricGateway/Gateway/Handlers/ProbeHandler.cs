using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Handlers
{
    public class ProbeHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.Compare(request.RequestUri.Segments[1].Replace("/", ""), "probe", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
