using System.Threading.Tasks;

namespace DiscoveryService.Infra.UnitOfWork
{
    public interface IServiceRegisterUnitOfWork
    {
        Task BeginTransaction();
        Task CommitTransaction();
    }
}
