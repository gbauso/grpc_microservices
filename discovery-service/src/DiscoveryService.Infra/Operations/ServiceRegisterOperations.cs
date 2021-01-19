using DiscoveryService.Infra.Database;
using DiscoveryService.Infra.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoveryService.Infra.Operations
{
    public class ServiceRegisterOperations : IServiceRegisterOperations
    {
        private readonly DiscoveryDbContext _discoveryDbContext;

        public ServiceRegisterOperations(DiscoveryDbContext discoveryDbContext)
        {
            _discoveryDbContext = discoveryDbContext;
        }

        public Task AddHandler(string grpcMethod, string handler)
        {
            var method = _discoveryDbContext.GrpcMethods.FirstOrDefault(i => i.Name == grpcMethod)
                ?? GrpcMethod.Create(grpcMethod);

            var service = _discoveryDbContext.Services.FirstOrDefault(i => i.Name == handler)
                ?? Service.Create(handler);

            var serviceMethod = ServiceMethod.Create(service, method);

            _discoveryDbContext.ServiceMethods.Add(serviceMethod);
            _discoveryDbContext.SaveChanges();

            return Task.CompletedTask;
        }

        public IEnumerable<ServiceMethod> GetAll()
        {
            return _discoveryDbContext.ServiceMethods.AsNoTracking().AsEnumerable();
        }

        public Task<IEnumerable<Service>> GetMethodHandlers(string grpcMethod)
        {
            var method = _discoveryDbContext.GrpcMethods.FirstOrDefault(i => i.Name == grpcMethod);

            if (method == null) return Task.FromResult(Enumerable.Empty<Service>());

            var services = _discoveryDbContext.ServiceMethods
                                    .Where(i => i.GrpcMethodId == method.Id && i.IsAlive)
                                    .Select(i => i.Service);

            return Task.FromResult(services.AsEnumerable());
        }

        public Task<IEnumerable<GrpcMethod>> GetServiceMethods(string serviceName)
        {
            var service = _discoveryDbContext.Services.FirstOrDefault(i => i.Name == serviceName);

            if (service == null) return Task.FromResult(Enumerable.Empty<GrpcMethod>());

            var services = _discoveryDbContext.ServiceMethods
                                    .Where(i => i.ServiceId == service.Id && i.IsAlive)
                                    .Select(i => i.GrpcMethod);

            return Task.FromResult(services.AsEnumerable());
        }

        public Task RemoveHandler(string grpcMethod, string handler)
        {
            var method = _discoveryDbContext.GrpcMethods.FirstOrDefault(i => i.Name == grpcMethod) ?? throw new Exception();
            var service = _discoveryDbContext.Services.FirstOrDefault(i => i.Name == handler) ?? throw new Exception();

            var handlerMethod = _discoveryDbContext.ServiceMethods
                                    .FirstOrDefault(
                                        i => i.GrpcMethodId == method.Id &&
                                        i.ServiceId == service.Id
                                    );

            _discoveryDbContext.ServiceMethods.Remove(handlerMethod);
            _discoveryDbContext.SaveChanges();

            return Task.CompletedTask;
        }

        public void SetServiceState(Guid serviceId, bool isAlive)
        {
            var serviceMethods = _discoveryDbContext.ServiceMethods
                                    .Where(i => i.ServiceId == serviceId);

            foreach (var serviceMethod in serviceMethods)
            {
                serviceMethod.IsAlive = isAlive;
                serviceMethod.LastHealthCheck = DateTime.UtcNow;
                _discoveryDbContext.ServiceMethods.Update(serviceMethod);
            }

            _discoveryDbContext.SaveChanges();
        }
    }
}
