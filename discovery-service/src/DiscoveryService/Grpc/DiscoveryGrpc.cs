using System.Threading.Tasks;
using Discovery;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DiscoveryService.Grpc
{
    public class DiscoveryGrpc : global::Discovery.DiscoveryService.DiscoveryServiceBase
    {
        private readonly EtcdClientWrap _etcdClient;
        private readonly ILogger<DiscoveryGrpc> _logger;

        public DiscoveryGrpc(EtcdClientWrap etcdClient, ILogger<DiscoveryGrpc> logger)
        {
            _etcdClient = etcdClient;
            _logger = logger;
        }
        
        public override Task<DiscoverySearchResponse> GetServiceHandlers(DiscoverySearchRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Request STARTED {request}", request);
            var response = new DiscoverySearchResponse();
            
            var services = _etcdClient.GetValue(request.ServiceDefinition)?.Split(';');
            if(!(services is null)) response.Services.AddRange(services);
            
            _logger.LogInformation("Request FINISHED {response}", response);
            return Task.FromResult(response);
        }
    }
}