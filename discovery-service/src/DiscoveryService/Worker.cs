using System.Threading;
using System.Threading.Tasks;
using DiscoveryService.Grpc;
using DiscoveryService.Util;
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
        private readonly MetricServer _metricServer;

        public Worker(IBusControl bus,
                      GrpcServerFactory grpcServerFactory,
                      ILogger<Worker> logger,
                      MetricsConfiguration metricsConfiguration)
        {
            _busControl = bus;
            _logger = logger;
            _server = grpcServerFactory.GetServer();
            _metricServer = new MetricServer(metricsConfiguration.Port);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _busControl.StartAsync(cancellationToken);
            _metricServer.Start();
            _server.Start();
            _logger.LogInformation("Discovery Service STARTED");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _busControl.StopAsync(cancellationToken);
            await _server.ShutdownAsync();
            await _metricServer.StopAsync();
            _logger.LogInformation("Discovery Service FINISHED");
        }
    }
}
