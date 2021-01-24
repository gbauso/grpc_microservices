using System.Threading.Tasks;

namespace Utils.Grpc.Metrics
{
    public interface IMetricsProvider
    {
        Task CollectCallMetrics(CallData data);
    }
}
