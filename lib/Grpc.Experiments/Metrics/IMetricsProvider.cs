using System.Threading.Tasks;

namespace Grpc.Experiments.Metrics
{
    public interface IMetricsProvider
    {
        Task CollectCallMetrics(CallData data);
    }
}
