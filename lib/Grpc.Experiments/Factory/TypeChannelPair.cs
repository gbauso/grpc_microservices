using Grpc.Core;
using System;

namespace Grpc.Experiments.Factory
{
    public class TypeChannelPair
    {
        public Channel Channel { get; set; }
        public Type ServiceType { get; set; }

        public static TypeChannelPair Create(Channel channel, Type type) => new TypeChannelPair
        {
            Channel = channel,
            ServiceType = type
        };
    }
}
