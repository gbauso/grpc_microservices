using System;
using System.Threading;
using System.Threading.Tasks;
using DiscoveryService.Grpc;
using DiscoveryService.HealthCheck;
using DiscoveryService.Infra.Database;
using DiscoveryService.Util;
using Grpc.Core;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;

namespace DiscoveryService
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IBusControl _busControl;
        private readonly Server _server;
        private Timer _timer;
        private readonly HealthChecker _healthChecker;
        private readonly MetricServer _metricServer;
        private readonly DiscoveryDbContext _discoveryDbContext;

        public Worker(IBusControl bus,
                      GrpcServerFactory grpcServerFactory,
                      ILogger<Worker> logger,
                      HealthChecker healthChecker,
                      DiscoveryDbContext discoveryDbContext,
                      IOptions<MetricsConfiguration> metricsConfiguration)
        {
            _busControl = bus;
            _logger = logger;
            _server = grpcServerFactory.GetServer();
            _metricServer = new MetricServer(metricsConfiguration.Value.Port);
            _discoveryDbContext = discoveryDbContext;
            _healthChecker = healthChecker;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discoveryDbContext.Database.Migrate();
            await _busControl.StartAsync(cancellationToken);
            _metricServer.Start();
            _server.Start();
            _timer = new Timer(_healthChecker.Handle, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5));
            _logger.LogInformation("Discovery Service STARTED");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _busControl.StopAsync(cancellationToken);
            await _server.ShutdownAsync();
            _timer?.Dispose();
            await _metricServer.StopAsync();
            _logger.LogInformation("Discovery Service FINISHED");
        }
    }
}
