using Cityinformation;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Utils.Grpc.DiscoveryClient;
using Utils.Grpc.Factory;
using Utils.Grpc.GrpcClients;
using Utils.Grpc.GrpcClients.Interceptors;
using Utils.Grpc.Metrics;
using Xunit;
using static Cityinformation.CityService;

namespace Utils.Grpc.Tests
{
    public class UnaryGrpcClientSingleTest
    {
        private Channel WeatherChannel = new Channel("localhost:10", ChannelCredentials.Insecure);
        private Channel NearbyCitiesChannel = new Channel("localhost:11", ChannelCredentials.Insecure);
        private Channel PopulationChannel = new Channel("localhost:12", ChannelCredentials.Insecure);
        private ChannelFactory _channelFactory;
        private ClientFactory _clientFactory;

        public UnaryGrpcClientSingleTest()
        {
            var channelFactoryMock = new Mock<ChannelFactory>(Mock.Of<IDiscoveryServiceClient>());
            channelFactoryMock
                .Setup(x => x.GetChannels(It.IsAny<string>()))
                .Returns(Task.FromResult(
                    new Channel[] { WeatherChannel , NearbyCitiesChannel , PopulationChannel }.AsEnumerable()
                ));

            _channelFactory = channelFactoryMock.Object;

            var clientFactoryMock = new Mock<ClientFactory>(new MetricsInterceptor(Mock.Of<IMetricsProvider>()));

            clientFactoryMock
                .Setup(x => x.GetClientInfo(It.IsAny<Type>()))
                .Returns(ServiceClientPair.Create("cityinformation.CityService", typeof(CityServiceClient)));


            Func<SearchResponse, CityServiceClient> serviceMock = (response) =>
            {
                var mock = new Mock<CityServiceClient>();
                mock
                    .Setup(x => x.GetCityInformationAsync(It.IsAny<SearchRequest>(), It.IsAny<CallOptions>()))
                    .Returns(new AsyncUnaryCall<SearchResponse>(Task.FromResult(response), null, null, null, null));

                return mock.Object;
            };


            clientFactoryMock
                .Setup(x => x.GetInstance(It.Is<TypeChannelPair>(x => x.Channel.Equals(WeatherChannel))))
                .Returns(serviceMock(new SearchResponse { Weather = "10" }));

            clientFactoryMock
                .Setup(x => x.GetInstance(It.Is<TypeChannelPair>(x => x.Channel.Equals(NearbyCitiesChannel))))
                .Returns(serviceMock(new SearchResponse { NearbyCities = "Berlin" }));

            clientFactoryMock
                .Setup(x => x.GetInstance(It.Is<TypeChannelPair>(x => x.Channel.Equals(PopulationChannel))))
                .Returns(serviceMock(new SearchResponse { Population = "123122" }));

            _clientFactory = clientFactoryMock.Object;
        }

        [Fact]
        public async Task UnaryGrpcClientSingle_ExecuteAndMerge_ShouldMergeResponsesInAUniqueObject()
        {
            var operation = new Operation { OperationId = Guid.NewGuid().ToString() };

            var sut = new UnaryGrpcClientSingle(_clientFactory,
                                                _channelFactory,
                                                operation,
                                                Mock.Of<ILogger<UnaryGrpcClientSingle>>());

            var request = new SearchRequest();
            var result = await sut.ExecuteAndMerge<SearchRequest, SearchResponse>(request);

            result.Should().NotBeNull();
            result.NearbyCities.Should().Be("Berlin");
            result.Population.Should().Be("123122");
            result.Weather.Should().Be("10");
        }
    }
}
