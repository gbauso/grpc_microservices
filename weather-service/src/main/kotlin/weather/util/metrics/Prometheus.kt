package weather.util.metrics

import io.prometheus.client.Counter
import io.prometheus.client.Histogram
import org.koin.core.KoinComponent
import io.prometheus.client.exporter.HTTPServer
import io.prometheus.client.hotspot.DefaultExports
import org.koin.core.inject
import weather.util.secrets.ISecretProvider

class Prometheus() : IMetricsProvider, KoinComponent {

    val secrets: ISecretProvider by inject()

    init {
        DefaultExports.initialize()
        val port = secrets.getValue("METRICS_PORT").toInt()
        HTTPServer(port)
    }

    override fun collectCallMetrics(metrics: CallMetrics) {
        with(grpcServerStartedTotal) {
            labels(metrics.callType, metrics.method)
                .inc()
        };

        with(grpcServerHandledTotal) {
            labels(metrics.callType, metrics.method, metrics.statusCode)
                .inc()
        };

        with(grpcServerHandlingSeconds) {
            labels(metrics.callType, metrics.method, metrics.statusCode)
                .observe(metrics.responseTime)
        }
    }

    companion object {
        val grpcServerStartedTotal: Counter = Counter
                .build()
                .name("grpc_server_started_total")
                .help("Total number of RPCs started on the server.")
                .labelNames("grpc_type", "grpc_method" )
                .register();
        val grpcServerHandledTotal: Counter = Counter
                .build()
                .name("grpc_server_handled_total")
                .help("Total number of RPCs completed on the server, regardless of success or failure.")
                .labelNames("grpc_type", "grpc_method", "grpc_code" )
                .register();
        val grpcServerHandlingSeconds: Histogram = Histogram
                .build()
                .name("grpc_server_handling_seconds")
                .help("""Histogram of response latency (seconds) of gRPC that had been application-level handled by the server.Duration of HTTP response size in bytes""")
                .labelNames("grpc_type", "grpc_method", "grpc_code" )
                .register();
    }

}