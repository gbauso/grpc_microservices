using System;

namespace DiscoveryService.Infra.Model
{
    public class GrpcMethod
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public static GrpcMethod Create(string name) => new GrpcMethod { Id = Guid.NewGuid(), Name = name };
    }
}
