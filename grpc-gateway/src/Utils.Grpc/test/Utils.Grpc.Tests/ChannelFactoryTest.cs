﻿using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Utils.Grpc.DiscoveryClient;
using Utils.Grpc.Factory;
using Xunit;
using Utils.Grpc.Extensions;
using FluentAssertions;
using Utils.Grpc.Interceptors;

namespace Utils.Grpc.Tests
{
    public class ChannelFactoryTest
    {
        private bool DiscoveryClientError = false;
        private Mock<IDiscoveryServiceClient> _discoveryServiceClientMock;
        private Mock<MetricsInterceptor> _metricsInterceptor;

        public ChannelFactoryTest()
        {
            var discoveryClientMock = new Mock<IDiscoveryServiceClient>();

            discoveryClientMock
                .Setup(x => x.GetHandlers(It.IsAny<string>()))
                .Returns(() => Task.FromResult(DiscoveryClientError ?
                                    throw new Exception()
                                    : Enumerable.Range(1, 3).Select(x => $"localhost:{x}").ToArray()
                                    ));

            _discoveryServiceClientMock = discoveryClientMock;
            _metricsInterceptor = new Mock<MetricsInterceptor>();
        }

        [Fact]
        public async Task ChannelFactory_Initialize_WhenDiscoveryServiceIsUp_ShoudStoreChannelsInstance()
        {
            var sut = new ChannelFactory(_discoveryServiceClientMock.Object, _metricsInterceptor.Object);

            var availableService = AppDomain.CurrentDomain.GetGrpcClients().First().GetServiceName();

            var channels = await sut.GetChannels(availableService);

            channels.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ChannelFactory_Initialize_WhenDiscoveryServiceIsUp_AndChannelNotExists_ShoudCreateANewOne()
        {
            var sut = new ChannelFactory(_discoveryServiceClientMock.Object, _metricsInterceptor.Object);

            var availableService = AppDomain.CurrentDomain.GetGrpcClients().First().GetServiceName();
            _discoveryServiceClientMock
                .Setup(x => x.GetHandlers(availableService))
                .Returns(Task.FromResult(Enumerable.Range(1, 4).Select(x => $"localhost:{x}").ToArray()));

            var channels = await sut.GetChannels(availableService);

            channels.Should().HaveCount(4);
        }

        [Fact]
        public void ChannelFactory_Initialize_WhenDiscoveryServiceIsDown_ShoudThrowAnException()
        {
            DiscoveryClientError = true;
            Action sut = () => new ChannelFactory(_discoveryServiceClientMock.Object, _metricsInterceptor.Object);

            sut.Should().Throw<Exception>();
        }
    }
}
