using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Net.Client;

namespace DiscoveryService.Factory
{
    public class ChannelFactory
    {
        private readonly IDictionary<string, GrpcChannel> _instances;

        public GrpcChannel GetChannel(string key)
        {
            if (_instances.ContainsKey(key))
            {
                return _instances[key];
            }
            else
            {
                var channel = GrpcChannel.ForAddress($"{key}:80");
                _instances[key] = channel;
                return channel;
            }
        }
    }
}
