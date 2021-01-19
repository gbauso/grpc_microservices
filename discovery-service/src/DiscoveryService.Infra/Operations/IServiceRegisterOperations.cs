using DiscoveryService.Infra.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscoveryService.Infra.Operations
{
    public interface IServiceRegisterOperations
    {
        IEnumerable<ServiceMethod> GetAll();
        void SetServiceState(Guid serviceId, bool isAlive);
        Task<IEnumerable<Service>> GetMethodHandlers(string grpcMethod);
        Task<IEnumerable<GrpcMethod>> GetServiceMethods(string service);
        Task AddHandler(string grpcMethod, string service);
        Task RemoveHandler(string grpcMethod, string service);
    }
}
