using System;
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
        private readonly Func<IServiceRegisterOperations> _serviceRegisteroperations;
        private readonly ILogger<DiscoveryGrpc> _logger;

        public DiscoveryGrpc(Func<IServiceRegisterOperations> serviceRegister, ILogger<DiscoveryGrpc> logger)
        {
            _serviceRegisteroperations = serviceRegister;
            _logger = logger;
        }
        
        public override async Task<DiscoverySearchResponse> GetServiceHandlers(DiscoverySearchRequest request, ServerCallContext context)
        {
            using(var operations = _serviceRegisteroperations())
            {
                _logger.LogInformation("Request STARTED {request}", request);
                var response = new DiscoverySearchResponse();

                var services = await operations.GetMethodHandlers(request.ServiceDefinition);
                if(!(services is null)) response.Services.AddRange(services.Select(i => i.Name));
            
                _logger.LogInformation("Request FINISHED {response}", response);
                return response;
            }
        }
    }
}