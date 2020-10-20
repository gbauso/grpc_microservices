using System.Linq;
using System.Threading.Tasks;
using Discovery;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace Application.DiscoveryClient
{
    public class DiscoveryServiceClient : IDiscoveryServiceClient
    {
        private readonly Channel _discoveryChannel;

        public DiscoveryServiceClient(IOptions<DiscoveryConfiguration> configuration)
        {
            _discoveryChannel = new Channel(configuration.Value.Url, ChannelCredentials.Insecure);
        }

        public async Task<string[]> GetHandlers(string service)
        {
            var client = new Discovery.DiscoveryService.DiscoveryServiceClient(_discoveryChannel);
            
            var request = new DiscoverySearchRequest { ServiceDefinition = service };

            return (await client.GetServiceHandlersAsync(request))?.Services.ToArray();
        }
    }
}