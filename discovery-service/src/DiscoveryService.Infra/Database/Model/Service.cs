using System;

namespace DiscoveryService.Infra.Model
{
    public class Service
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public static Service Create(string name) => new Service { Id = Guid.NewGuid(), Name = name };
    }
}
