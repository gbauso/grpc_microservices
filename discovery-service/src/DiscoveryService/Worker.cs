using System.Threading;
using System.Threading.Tasks;
using DiscoveryService.Grpc;
using Grpc.Core;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace DiscoveryService
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IBusControl _busControl;
        private readonly Server _server;

        public Worker(IBusControl bus, GrpcServerFactory grpcServerFactory, ILogger<Worker> logger)
        {
            _busControl = bus;
            _logger = logger;
            _server = grpcServerFactory.GetServer();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            new MetricServer(port: 3004).Start();
            await _busControl.StartAsync(cancellationToken);
            _server.Start();
            _logger.LogInformation("Discovery Service STARTED");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _busControl.StopAsync(cancellationToken);
            await _server.ShutdownAsync();
            _logger.LogInformation("Discovery Service FINISHED");
        }
    }
}
