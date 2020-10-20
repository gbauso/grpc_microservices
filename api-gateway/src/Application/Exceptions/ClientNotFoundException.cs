using System;

namespace Application.Exceptions
{
    public class ClientNotFoundException : Exception
    {
        public ClientNotFoundException() : base("The client was not found")
        {
            
        }
    }
}