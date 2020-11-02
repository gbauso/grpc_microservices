using Metrics.Model;
using System.Threading.Tasks;

namespace Metrics.Providers
{
    public interface IMetricsProvider
    {
        Task CollectCallMetrics(CallData data);
        Task CollectServerMetrics(ServerData data);
    }
}
