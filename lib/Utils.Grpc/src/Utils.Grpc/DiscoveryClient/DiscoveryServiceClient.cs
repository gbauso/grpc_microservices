using System;
using System.Linq;
using System.Threading.Tasks;
using Discovery;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;
using Utils.Grpc.Interceptors;

namespace Utils.Grpc.DiscoveryClient
{
    internal class DiscoveryServiceClient : IDiscoveryServiceClient
    {
        private readonly Channel _discoveryChannel;
        private readonly Interceptor _metricsInterceptor;
        private readonly Operation _operation;
        private readonly int _timeout;

        public DiscoveryServiceClient(IOptions<DiscoveryConfiguration> configuration,
                                      Operation operation,
                                      MetricsInterceptor interceptor)
        {
            _discoveryChannel = new Channel(configuration.Value.Url, ChannelCredentials.Insecure);
            _timeout = configuration.Value.Timeout;
            _metricsInterceptor = interceptor;
            _operation = operation;
        }

        public async Task<string[]> GetHandlers(string service)
        {
            var client = new DiscoveryService.DiscoveryServiceClient(_discoveryChannel);
            
            var request = new DiscoverySearchRequest { ServiceDefinition = service };

            return (await client.GetServiceHandlersAsync(request, GetCallContext()))?.Services.ToArray();
        }

        private CallOptions GetCallContext()
        {
            var headers = new Metadata
            {
                {"operation_id", _operation.OperationId.ToString()},
            };

            return new CallOptions(headers, DateTime.UtcNow.AddSeconds(_timeout));
        }
    }
}