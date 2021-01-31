using Grpc.Core;
using System;

namespace Utils.Grpc.Mediator.Factory
{
    internal class TypeChannelPair
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
