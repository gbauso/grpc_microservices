using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Application.DiscoveryClient;
using Application.Exceptions;
using Grpc.Core;

namespace Application.Factory
{
    public class ChannelFactory
    {
        private readonly IDiscoveryServiceClient _discoveryServiceClient;
        private readonly IDictionary<string, Channel> _instances;

        public ChannelFactory(IDiscoveryServiceClient discoveryServiceClient)
        {
            _discoveryServiceClient = discoveryServiceClient;
            _instances = new Dictionary<string, Channel>();
            Initialize().Wait();
        }

        #region Initialization

        private async Task Initialize()
        {
            var clients = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(i => i.Name.EndsWith("ServiceClient") && !i.Name.Contains("Discovery"));

            foreach (var client in clients)
            {
                var serviceName = client.ReflectedType.GetRuntimeFields().First().GetValue(null).ToString();
                var handlers = await _discoveryServiceClient.GetHandlers(serviceName);
                RegisterHandlers(handlers);
            }
        }

        private void RegisterHandlers(IEnumerable<string> handlers)
        {
            foreach (var handler in handlers)
            {
                if (!_instances.ContainsKey(handler))
                    _instances[handler] = new Channel($"{handler}", ChannelCredentials.Insecure);
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
            if (_instances.ContainsKey(handler))
                return _instances[handler];
            else
                throw new ChannelNotFoundException();
        }
    }
}