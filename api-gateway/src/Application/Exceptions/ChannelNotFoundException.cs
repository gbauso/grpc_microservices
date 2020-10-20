using System;

namespace Application.Exceptions
{
    public class ChannelNotFoundException : Exception
    {
        public ChannelNotFoundException() : base("The channel was not found")
        {
            
        }
    }
}