using System;
using System.Linq;
using System.Threading.Tasks;
using Discovery;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utils.Grpc.Interceptors;
using Utils.Grpc.Extensions;

namespace Utils.Grpc.DiscoveryClient
{
    internal class DiscoveryServiceClient : IDiscoveryServiceClient
    {
        private readonly Channel _discoveryChannel;
        private readonly int _timeout;
        private readonly ILogger<DiscoveryServiceClient> _logger;

        public DiscoveryServiceClient(IOptions<DiscoveryConfiguration> configuration,
                                      MetricsInterceptor interceptor,
                                      ILogger<DiscoveryServiceClient> logger)
        {
            _discoveryChannel = new Channel(configuration.Value.Url, ChannelCredentials.Insecure);
            _discoveryChannel.Intercept(interceptor);
            _timeout = configuration.Value.Timeout;
            _logger = logger;
        }

        public async Task<string[]> GetHandlers(string service)
        {
            var correlationId = Guid.NewGuid().ToString();

            var client = new DiscoveryService.DiscoveryServiceClient(_discoveryChannel);
            
            var request = new DiscoverySearchRequest { ServiceDefinition = service };

            var callContext = client.GetCallOptions(correlationId, "getServiceHandlers", _discoveryChannel.ResolvedTarget);

            return (await client.GetServiceHandlersAsync(request, callContext))?.Services.ToArray();
        }
    }
}