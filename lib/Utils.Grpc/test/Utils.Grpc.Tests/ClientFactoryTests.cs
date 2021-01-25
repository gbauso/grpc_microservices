using Moq;
using System;
using System.Linq;
using Utils.Grpc.Factory;
using Xunit;
using Utils.Grpc.Extensions;
using FluentAssertions;
using Utils.Grpc.GrpcClients.Interceptors;
using Cityinformation;
using Utils.Grpc.Metrics;
using static Cityinformation.CityService;
using Grpc.Core;

namespace Utils.Grpc.Tests
{
    public class ClientFactoryTest
    {
        private MetricsInterceptor MetricsInterceptor;

        public ClientFactoryTest()
        {
            MetricsInterceptor = new MetricsInterceptor(Mock.Of<IMetricsProvider>());
        }

        [Fact]
        public void ClientFactory_Initialize_WhenExists_ShouldStoreClientInformation()
        {
            var sut = new ClientFactory(MetricsInterceptor);

            var clientInfo = sut.GetClientInfo(typeof(SearchResponse));

            var availableService = AppDomain.CurrentDomain.GetGrpcClients().First().GetServiceName();

            clientInfo.Should().NotBeNull();
            clientInfo.ServiceType.Should().Be(typeof(CityServiceClient));
            clientInfo.ServiceName.Should().Be(availableService);

            var channel = new Channel("localhost", ChannelCredentials.Insecure);
            var typeChannel = TypeChannelPair.Create(channel , typeof(CityServiceClient));
            var client = sut.GetInstance(typeChannel);

            client.Should().NotBeNull();
        }

        [Fact]
        public void ClientFactory_GetInstance_ShouldRetunClient()
        {
            var sut = new ClientFactory(MetricsInterceptor);

            var channel = new Channel("localhost", ChannelCredentials.Insecure);
            var typeChannel = TypeChannelPair.Create(channel, typeof(CityServiceClient));
            var client = sut.GetInstance(typeChannel);

            client.Should().NotBeNull();
        }

    }
}
