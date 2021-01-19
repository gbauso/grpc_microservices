using System.Linq;
using System.Threading.Tasks;
using Discovery;
using DiscoveryService.Infra.Operations;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DiscoveryService.Grpc
{
    public class DiscoveryGrpc : global::Discovery.DiscoveryService.DiscoveryServiceBase
    {
        private readonly IServiceRegisterOperations _serviceRegisterRepository;
        private readonly ILogger<DiscoveryGrpc> _logger;

        public DiscoveryGrpc(IServiceRegisterOperations serviceRegister, ILogger<DiscoveryGrpc> logger)
        {
            _serviceRegisterRepository = serviceRegister;
            _logger = logger;
        }
        
        public override async Task<DiscoverySearchResponse> GetServiceHandlers(DiscoverySearchRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Request STARTED {request}", request);
            var response = new DiscoverySearchResponse();

            var services = await _serviceRegisterRepository.GetMethodHandlers(request.ServiceDefinition);
            if(!(services is null)) response.Services.AddRange(services.Select(i => i.Name));
            
            _logger.LogInformation("Request FINISHED {response}", response);
            return response;
        }
    }
}