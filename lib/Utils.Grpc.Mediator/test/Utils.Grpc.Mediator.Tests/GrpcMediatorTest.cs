using Cityinformation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Utils.Grpc.Mediator.Factory;
using Utils.Grpc.Mediator.GrpcClients;
using Utils.Grpc.Mediator.GrpcClients.Interceptors;
using Utils.Grpc.Mediator.Metrics;
using Xunit;
using static Cityinformation.CityService;

namespace Utils.Grpc.Mediator.Tests
{
    public class GrpcMediatorTest
    {
        private IServiceProvider _serviceProvider;

        public GrpcMediatorTest()
        {
            var clientFactoryMock = new Mock<ClientFactory>(new MetricsInterceptor(Mock.Of<IMetricsProvider>()));

            clientFactoryMock
                .Setup(x => x.GetClientInfo(It.IsAny<Type>()))
                .Returns(ServiceClientPair.Create("cityinformation.CityService", typeof(CityServiceClient)));

            var unarySingle = new Mock<UnaryGrpcClientSingle>();
            unarySingle
                .Setup(x => x.ExecuteAndMerge<SearchRequest, SearchResponse>(It.IsAny<SearchRequest>()))
                .Returns(Task.FromResult(new SearchResponse()));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(ClientFactory)))
                .Returns(clientFactoryMock.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(UnaryGrpcClientSingle)))
                .Returns(unarySingle.Object);

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            _serviceProvider = serviceProvider.Object;
        }

        [Fact]
        public async Task GrpcMediatorTest_Execute_CallUnaryMethod()
        {
            var sut = new GrpcMediator(_serviceProvider);

            var request = new SearchRequest();
            var response = await sut.Send<SearchRequest, SearchResponse>(request);

            response.Should().NotBeNull();
        }
    }
}
