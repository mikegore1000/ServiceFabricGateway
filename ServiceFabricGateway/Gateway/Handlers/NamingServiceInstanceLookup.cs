using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;

namespace Gateway.Handlers
{
    internal class NamingServiceInstanceLookup : IServiceInstanceLookup
    {
        // TODO: Can't see a way to test this without reflection hacks!
        // See https://github.com/loekd/ServiceFabric.Mocks
        public async Task<Uri> GetAddress(string fabricAddress, CancellationToken cancellationToken)
        {
            try
            {
                var resolved = await ServicePartitionResolver.GetDefault().ResolveAsync(
                    new Uri(fabricAddress),
                    new ServicePartitionKey(),
                    ServicePartitionResolver.DefaultResolveTimeout,
                    ServicePartitionResolver.DefaultMaxRetryBackoffInterval,
                    cancellationToken
                );

                JObject addresses = JObject.Parse(resolved.GetEndpoint().Address);
                return new Uri((string)addresses["Endpoints"].First());
            }
            catch (FabricServiceNotFoundException)
            {
                return null;
            }
        }
    }
}