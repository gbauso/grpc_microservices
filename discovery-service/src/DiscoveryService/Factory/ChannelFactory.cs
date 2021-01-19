using Grpc.Core;
using System.Collections.Generic;

namespace DiscoveryService.Factory
{
    public class ChannelFactory
    {
        private readonly IDictionary<string, Channel> _instances = new Dictionary<string, Channel>();

        public virtual Channel GetChannel(string key)
        {
            if (_instances.ContainsKey(key))
            {
                return _instances[key];
            }
            else
            {
                var channel = new Channel(key, ChannelCredentials.Insecure);
                _instances[key] = channel;
                return channel;
            }
        }
    }
}
