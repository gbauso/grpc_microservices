using System.Threading.Tasks;

namespace Utils.Grpc.DiscoveryClient
{
    internal interface IDiscoveryServiceClient
    {
        Task<string[]> GetHandlers(string service);
    }
}