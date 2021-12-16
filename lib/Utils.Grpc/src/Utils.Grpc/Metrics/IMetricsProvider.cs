using System.Threading.Tasks;

namespace Utils.Grpc.Metrics
{
    internal interface IMetricsProvider
    {
        Task CollectCallMetrics(CallData data);
    }
}
