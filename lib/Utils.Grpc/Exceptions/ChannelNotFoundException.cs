using System;

namespace Utils.Grpc.Exceptions
{
    public class ChannelNotFoundException : Exception
    {
        public ChannelNotFoundException() : base("The channel was not found")
        {
            
        }
    }
}