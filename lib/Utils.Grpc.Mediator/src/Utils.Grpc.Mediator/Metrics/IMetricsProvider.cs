using System.Threading.Tasks;

namespace Utils.Grpc.Mediator.Metrics
{
    internal interface IMetricsProvider
    {
        Task CollectCallMetrics(CallData data);
    }
}
