using System.Threading.Tasks;

namespace Grpc.Experiments.DiscoveryClient
{
    public interface IDiscoveryServiceClient
    {
        Task<string[]> GetHandlers(string service);
    }
}