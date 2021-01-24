using System.Threading.Tasks;

namespace Utils.Grpc.DiscoveryClient
{
    public interface IDiscoveryServiceClient
    {
        Task<string[]> GetHandlers(string service);
    }
}