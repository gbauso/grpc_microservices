using Cityinformation;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Utils.Grpc.Mediator.DiscoveryClient;
using Utils.Grpc.Mediator.Factory;
using Utils.Grpc.Mediator.GrpcClients;
using Utils.Grpc.Mediator.GrpcClients.Interceptors;
using Utils.Grpc.Mediator.Metrics;
using Xunit;
using static Cityinformation.CityService;

namespace Utils.Grpc.Mediator.Tests
{
    public class UnaryGrpcClientSingleTest
    {
        private Channel WeatherChannel = new Channel("localhost:10", ChannelCredentials.Insecure);
        private Channel NearbyCitiesChannel = new Channel("localhost:11", ChannelCredentials.Insecure);
        private Channel PopulationChannel = new Channel("localhost:12", ChannelCredentials.Insecure);
        private ChannelFactory _channelFactory;
        private ClientFactory _clientFactory;
        private GrpcRetryPolicy _grpcRetryPolicy;
        private bool forceGrpcError = false;

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


            Func<SearchResponse, bool, CityServiceClient> serviceMock = (response, forceError) =>
            {
                var mock = new Mock<CityServiceClient>();
                mock
                    .Setup(x => x.GetCityInformationAsync(It.IsAny<SearchRequest>(), It.IsAny<CallOptions>()))
                    .Returns(forceError ? 
                        throw new Exception()
                        : new AsyncUnaryCall<SearchResponse>(Task.FromResult(response), null, null, null, null)
                    );

                return mock.Object;
            };


            clientFactoryMock
                .Setup(x => x.GetInstance(It.Is<TypeChannelPair>(x => x.Channel.Equals(WeatherChannel))))
                .Returns(() => serviceMock(new SearchResponse { Weather = "10" }, false));

            clientFactoryMock
                .Setup(x => x.GetInstance(It.Is<TypeChannelPair>(x => x.Channel.Equals(NearbyCitiesChannel))))
                .Returns(() => serviceMock(new SearchResponse { NearbyCities = "Berlin" }, forceGrpcError));

            clientFactoryMock
                .Setup(x => x.GetInstance(It.Is<TypeChannelPair>(x => x.Channel.Equals(PopulationChannel))))
                .Returns(() => serviceMock(new SearchResponse { Population = "123122" }, false));

            _clientFactory = clientFactoryMock.Object;
            _grpcRetryPolicy = new GrpcRetryPolicy(Mock.Of<ILogger<GrpcRetryPolicy>>());
        }

        [Fact]
        public async Task UnaryGrpcClientSingle_ExecuteAndMerge_FullContent_ShouldMergeResponsesInAnUniqueObject()
        {
            var operation = new Operation { OperationId = Guid.NewGuid().ToString() };

            var sut = new UnaryGrpcClientSingle(_clientFactory,
                                                _channelFactory,
                                                operation,
                                                _grpcRetryPolicy,
                                                Mock.Of<ILogger<UnaryGrpcClientSingle>>());

            var request = new SearchRequest();
            var result = await sut.ExecuteAndMerge<SearchRequest, SearchResponse>(request);

            result.Should().NotBeNull();
            result.PartialContent.Should().BeFalse();
            result.Content.NearbyCities.Should().Be("Berlin");
            result.Content.Population.Should().Be("123122");
            result.Content.Weather.Should().Be("10");
        }

        [Fact]
        public async Task UnaryGrpcClientSingle_ExecuteAndMerge_PartialContent_ShouldMergeAvailableResponsesInAnUniqueObject()
        {
            forceGrpcError = true;
            var operation = new Operation { OperationId = Guid.NewGuid().ToString() };

            var sut = new UnaryGrpcClientSingle(_clientFactory,
                                                _channelFactory,
                                                operation,
                                                _grpcRetryPolicy,
                                                Mock.Of<ILogger<UnaryGrpcClientSingle>>());

            var request = new SearchRequest();
            var result = await sut.ExecuteAndMerge<SearchRequest, SearchResponse>(request);

            result.Should().NotBeNull();
            result.PartialContent.Should().BeTrue();
            result.Content.NearbyCities.Should().BeNullOrEmpty();
            result.Content.Population.Should().Be("123122");
            result.Content.Weather.Should().Be("10");
        }
    }
}
