using System;

namespace Utils.Grpc.Factory
{
    public class ServiceClientPair
    {
        public string ServiceName { get; set; }
        public Type ServiceType { get; set; }

        public static ServiceClientPair Create(string service, Type type) => new ServiceClientPair
        {
            ServiceName = service,
            ServiceType = type
        };
    }
}
