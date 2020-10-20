using System.Threading.Tasks;

namespace Application.DiscoveryClient
{
    public interface IDiscoveryServiceClient
    {
        Task<string[]> GetHandlers(string service);
    }
}