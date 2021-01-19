using Prometheus;
using System.Threading.Tasks;

namespace Application.Metrics
{
    public class PrometheusMetrics : IMetricsProvider
    {
        private readonly Counter grpcServerStartedTotal;
        private readonly Counter grpcServerHandledTotal;
        private readonly Histogram grpcServerHandlingSeconds;

        public PrometheusMetrics()
        {
            grpcServerStartedTotal = Prometheus.Metrics
                .CreateCounter("grpc_server_started_total",
                               "Total number of RPCs started on the server.",
                               new CounterConfiguration
                               {
                                   LabelNames = new [] { "grpc_type", "grpc_method" }
                               }); 

            grpcServerHandledTotal = Prometheus.Metrics
                .CreateCounter("grpc_server_handled_total",
                               "Total number of RPCs completed on the server, regardless of success or failure.",
                               new CounterConfiguration
                               {
                                   LabelNames = new[] { "grpc_type", "grpc_method", "grpc_code" }
                               });

            grpcServerHandlingSeconds = Prometheus.Metrics
                .CreateHistogram("grpc_server_handling_seconds",
                                 @"Histogram of response latency (seconds) of gRPC that had been application-level handled
                                   by the server.Duration of HTTP response size in bytes",
                                 new HistogramConfiguration
                                 {
                                     LabelNames = new[] { "grpc_type", "grpc_method", "grpc_code" }
                                 }); 
        }

        public Task CollectCallMetrics(CallData data)
        {
            grpcServerStartedTotal.WithLabels(data.CallType, data.Method).Inc();
            grpcServerHandledTotal.WithLabels(data.CallType, data.Method, data.Status).Inc();
            grpcServerHandlingSeconds.WithLabels(data.CallType, data.Method, data.Status).Observe(data.Duration);

            return Task.CompletedTask;
        }
    }
}
