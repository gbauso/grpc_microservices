using System.Threading.Tasks;

namespace Application.Metrics
{
    public interface IMetricsProvider
    {
        Task CollectCallMetrics(CallData data);
    }
}
