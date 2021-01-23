using System;

namespace Grpc.Experiments.Exceptions
{
    public class ChannelNotFoundException : Exception
    {
        public ChannelNotFoundException() : base("The channel was not found")
        {
            
        }
    }
}