using DiscoveryService.Factory;
using DiscoveryService.HealthCheck;
using DiscoveryService.Infra.Database;
using DiscoveryService.Infra.Operations;
using DiscoveryService.Test.Stub;
using FluentAssertions;
using Grpc.Core;
using Healthcheck;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Healthcheck.HealthCheckService;

namespace DiscoveryService.Test
{
    public class HealthCheckerTest
    {
        private readonly Func<ServiceRegisterOperationsStub> _operations;
        private readonly ChannelFactory _channelFactory;
        private readonly Func<Channel, HealthCheckServiceClient> _serviceClientFactory;
        private bool forceGrpcError = false;

        public HealthCheckerTest()
        {
            var discoveryDbContext = new DiscoveryDbContext(new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

            var operation = new ServiceRegisterOperationsStub(discoveryDbContext);
            _operations = () => operation;

            var channelFactoryMock = new Mock<ChannelFactory>();
            channelFactoryMock
                .Setup(x => x.GetChannel(It.IsAny<string>()))
                .Returns(new Channel("abc", ChannelCredentials.Insecure));

            _channelFactory = channelFactoryMock.Object;

            var clientMock = new Mock<HealthCheckServiceClient>();
            clientMock.Setup(i => i.GetStatus(It.IsAny<Empty>(),
                                              It.IsAny<CallOptions>()))
                .Returns(() => forceGrpcError ? throw new Exception() : new PingResponse { Response = "Pong" });

            _serviceClientFactory = (Channel ch) => clientMock.Object;

            var preOperation = _operations();
            preOperation.AddHandler("svc", "population");
            preOperation.AddHandler("svc", "weather");
        }

        [Theory]
        [InlineData("population")]
        [InlineData("weather")]
        public async Task HealthChecker_Handle_ServicesDown(string serviceToCheckIfIsDown)
        {
            forceGrpcError = true;
            var healthChecker = new HealthChecker(_operations, _channelFactory, _serviceClientFactory);

            healthChecker.Handle(null);

            var services = await _operations().GetMethodHandlers("svc");

            services.Select(i => i.Name).Should().NotContain(serviceToCheckIfIsDown);
        }

        [Theory]
        [InlineData("population")]
        [InlineData("weather")]
        public async Task HealthChecker_Handle_ServicesUp(string serviceToCheckIfIsUp)
        {
            forceGrpcError = false;
            var healthChecker = new HealthChecker(_operations, _channelFactory, _serviceClientFactory);

            healthChecker.Handle(null);

            var services = await _operations().GetMethodHandlers("svc");

            services.Select(i => i.Name).Should().Contain(serviceToCheckIfIsUp);
        }
    } 
}
