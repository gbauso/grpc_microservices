using System;
using System.Threading;
using System.Threading.Tasks;
using DiscoveryService.Grpc;
using DiscoveryService.HealthCheck;
using Grpc.Core;
using Healthcheck;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscoveryService
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IBusControl _busControl;
        private readonly Server _server;
        private Timer _timer;
        private readonly HealthChecker _healthChecker;

        public Worker(IBusControl bus, GrpcServerFactory grpcServerFactory, ILogger<Worker> logger)
        {
            _busControl = bus;
            _logger = logger;
            _server = grpcServerFactory.GetServer();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _busControl.StartAsync(cancellationToken);
            _server.Start();
            _timer = new Timer(_healthChecker.Handle, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Discovery Service STARTED");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _busControl.StopAsync(cancellationToken);
            await _server.ShutdownAsync();
            _timer?.Dispose();
            _logger.LogInformation("Discovery Service FINISHED");
        }
    }
}
