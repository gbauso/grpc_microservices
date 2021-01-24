using System;

namespace Utils.Grpc.Exceptions
{
    public class ClientNotFoundException : Exception
    {
        public ClientNotFoundException() : base("The client was not found")
        {
            
        }
    }
}