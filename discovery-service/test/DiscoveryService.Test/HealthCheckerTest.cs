using DiscoveryService.Factory;
using DiscoveryService.HealthCheck;
using DiscoveryService.Infra.Database;
using DiscoveryService.Infra.Operations;
using DiscoveryService.Infra.UnitOfWork;
using FluentAssertions;
using Grpc.Core;
using Healthcheck;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Healthcheck.HealthCheckService;

namespace DiscoveryService.Test
{
    public class HealthCheckerTest
    {
        private readonly IServiceRegisterOperations _operations;
        private readonly IServiceRegisterUnitOfWork _uow;
        private readonly ChannelFactory _channelFactory;
        private readonly DiscoveryDbContext _discoveryDbContext;
        private readonly Func<Channel, HealthCheckServiceClient> _serviceClientFactory;
        private bool forceGrpcError = false;

        public HealthCheckerTest()
        {
            _discoveryDbContext = new DiscoveryDbContext(new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

            _operations = new ServiceRegisterOperations(_discoveryDbContext);
            _uow = new ServiceRegisterUnitOfWork(_discoveryDbContext);
            _channelFactory = Mock.Of<ChannelFactory>();

            var clientMock = new Mock<HealthCheckServiceClient>();
            clientMock.Setup(i => i.GetStatus(It.IsAny<Empty>(),
                                              null, null, default))
                .Returns(() => forceGrpcError ? throw new Exception() : new PingResponse { Response = "Pong" });

            _serviceClientFactory = (Channel ch) => clientMock.Object;

            _operations.AddHandler("svc", "population");
            _operations.AddHandler("svc", "weather");
        }

        [Theory]
        [InlineData("population")]
        [InlineData("weather")]
        public async Task HealthChecker_Handle_ServicesDown(string serviceToCheckIfIsDown)
        {
            forceGrpcError = true;
            var healthChecker = new HealthChecker(_operations, _uow, _channelFactory, _serviceClientFactory);

            healthChecker.Handle(null);

            var services = await _operations.GetMethodHandlers("svc");

            services.Select(i => i.Name).Should().NotContain(serviceToCheckIfIsDown);
        }

        [Theory]
        [InlineData("population")]
        [InlineData("weather")]
        public async Task HealthChecker_Handle_ServicesUp(string serviceToCheckIfIsUp)
        {
            forceGrpcError = false;
            var healthChecker = new HealthChecker(_operations, _uow, _channelFactory, _serviceClientFactory);

            healthChecker.Handle(null);

            var services = await _operations.GetMethodHandlers("svc");

            services.Select(i => i.Name).Should().Contain(serviceToCheckIfIsUp);
        }
    } 
}
