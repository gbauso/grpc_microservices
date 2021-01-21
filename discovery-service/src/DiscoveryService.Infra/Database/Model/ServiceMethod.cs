using System;

namespace DiscoveryService.Infra.Model
{
    public class ServiceMethod
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public Guid GrpcMethodId { get; set; }
        public DateTime LastHealthCheck { get; set; }
        public bool IsAlive { get; set; }

        public virtual Service Service { get; set; }
        public virtual GrpcMethod GrpcMethod { get; set; }

        public static ServiceMethod Create(Service service, GrpcMethod method)
            => new ServiceMethod
            {
                Id = Guid.NewGuid(),
                Service = service,
                ServiceId = service.Id,
                GrpcMethodId = method.Id,
                GrpcMethod = method,
                LastHealthCheck = DateTime.UtcNow,
                IsAlive = true
            };
    }
}
