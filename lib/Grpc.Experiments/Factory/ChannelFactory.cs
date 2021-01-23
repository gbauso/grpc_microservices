using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Experiments.DiscoveryClient;
using Grpc.Experiments.Exceptions;
using Grpc.Core;
using Grpc.Experiments.Extensions;
using System.Collections.Concurrent;

namespace Grpc.Experiments.Factory
{
    public class ChannelFactory
    {
        private readonly IDiscoveryServiceClient _discoveryServiceClient;
        private readonly IDictionary<string, Channel> _instances;

        public ChannelFactory(IDiscoveryServiceClient discoveryServiceClient)
        {
            _discoveryServiceClient = discoveryServiceClient;
            _instances = new ConcurrentDictionary<string, Channel>();
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


        public async Task<IEnumerable<Channel>> GetChannels(string service)
        {
            var handlers = await _discoveryServiceClient.GetHandlers(service);

            return handlers?.Select(GetChannel);
        }

        private Channel GetChannel(string handler)
        {
            if (_instances.ContainsKey(handler) && _instances.TryGetValue(handler, out Channel channel))
                return channel;
            else
                throw new ChannelNotFoundException();
        }
    }
}