using System;

namespace Utils.Grpc.Mediator.Exceptions
{
    public class ClientNotFoundException : Exception
    {
        public ClientNotFoundException() : base("The client was not found")
        {
            
        }
    }
}