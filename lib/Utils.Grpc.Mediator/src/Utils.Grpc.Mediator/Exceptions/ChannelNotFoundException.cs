using System;

namespace Utils.Grpc.Mediator.Exceptions
{
    public class ChannelNotFoundException : Exception
    {
        public ChannelNotFoundException() : base("The channel was not found")
        {
            
        }
    }
}