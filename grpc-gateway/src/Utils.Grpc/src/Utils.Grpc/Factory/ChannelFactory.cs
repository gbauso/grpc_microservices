﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Grpc.DiscoveryClient;
using Utils.Grpc.Exceptions;
using Grpc.Core;
using Utils.Grpc.Extensions;
using System.Collections.Concurrent;
using Utils.Grpc.Interceptors;
using Grpc.Core.Interceptors;

namespace Utils.Grpc.Factory
{
    public interface IChannelFactory
    {
        Task<IEnumerable<Channel>> GetChannels(string service);
    }
    internal class ChannelFactory : IChannelFactory
    {
        private readonly IDiscoveryServiceClient _discoveryServiceClient;
        private readonly IDictionary<string, Channel> _instances;
        private readonly MetricsInterceptor _metricsInterceptor;

        public ChannelFactory(IDiscoveryServiceClient discoveryServiceClient, MetricsInterceptor metricsInterceptor)
        {
            _discoveryServiceClient = discoveryServiceClient;
            _instances = new ConcurrentDictionary<string, Channel>();
            _metricsInterceptor = metricsInterceptor;
            Initialize().Wait();
        }

        #region Initialization

        private async Task Initialize()
        {
            var clients = AppDomain.CurrentDomain.GetGrpcClients();

            foreach (var client in clients)
            {
                var serviceName = client.GetServiceName();
                var handlers = await _discoveryServiceClient.GetHandlers(serviceName);
                RegisterHandlers(handlers);
            }
        }

        private void RegisterHandlers(IEnumerable<string> handlers)
        {
            foreach (var handler in handlers)
            {
                if (!_instances.ContainsKey(handler))
                    _instances[handler] = new Channel(handler, ChannelCredentials.Insecure);
            }
        }

        #endregion


        public async virtual Task<IEnumerable<Channel>> GetChannels(string service)
        {
            var handlers = await _discoveryServiceClient.GetHandlers(service);

            return handlers?.Select(GetChannel);
        }

        private Channel GetChannel(string handler)
        {
            if (_instances.ContainsKey(handler) && _instances.TryGetValue(handler, out Channel channel))
            {
                channel.Intercept(_metricsInterceptor);
                return channel;
            }
            else
            {
                RegisterHandlers(new[] { handler });
                return GetChannel(handler);
            }
        }
    }
}