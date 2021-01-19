using DiscoveryService.Infra.Database;
using System;
using System.Threading.Tasks;

namespace DiscoveryService.Infra.UnitOfWork
{
    public class ServiceRegisterUnitOfWork : IServiceRegisterUnitOfWork
    {
        private readonly DiscoveryDbContext _discoveryDbContext;

        public ServiceRegisterUnitOfWork(DiscoveryDbContext discoveryDbContext)
        {
            _discoveryDbContext = discoveryDbContext;
        }

        public async Task BeginTransaction()
        {
            await _discoveryDbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransaction()
        {
            await _discoveryDbContext.Database.CommitTransactionAsync();
        }
    }
}
