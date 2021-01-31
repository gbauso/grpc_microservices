using System.Threading.Tasks;

namespace Utils.Grpc.Mediator.DiscoveryClient
{
    internal interface IDiscoveryServiceClient
    {
        Task<string[]> GetHandlers(string service);
    }
}