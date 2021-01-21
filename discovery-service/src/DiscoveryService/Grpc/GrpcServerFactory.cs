using Grpc.Core;
using Microsoft.Extensions.Options;

namespace DiscoveryService.Grpc
{
    public class GrpcServerFactory
    {
        private readonly DiscoveryGrpc _service;
        private readonly IOptions<GrpcConfiguration> _configuration;

        public GrpcServerFactory(DiscoveryGrpc service, IOptions<GrpcConfiguration> configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        public Server GetServer()
        {
            var configuration = _configuration.Value;

            return new Server
            {
                Services = { global::Discovery.DiscoveryService.BindService(_service) },
                Ports = { new ServerPort(configuration.Host, configuration.Port, ServerCredentials.Insecure) }
            };
        }
    }
}