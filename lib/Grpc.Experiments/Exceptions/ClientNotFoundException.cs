using System;

namespace Grpc.Experiments.Exceptions
{
    public class ClientNotFoundException : Exception
    {
        public ClientNotFoundException() : base("The client was not found")
        {
            
        }
    }
}