package weather.interceptors

import io.grpc.ForwardingServerCall
import io.grpc.Metadata
import io.grpc.ServerCall
import io.grpc.Status
import weather.util.metrics.CallMetrics
import weather.util.metrics.IMetricsProvider
import java.util.*

class MetricsForwardingServerCall<ReqT, RespT>(delegate: ServerCall<ReqT, RespT>?, private var metrics: IMetricsProvider, private var start: Date, private var headers: Metadata?) : ForwardingServerCall.SimpleForwardingServerCall<ReqT, RespT>(delegate) {
    override fun close(status: Status?, trailers: Metadata?) {
        val metadata = headers?.asMap()
        val timeElapsed = Date().time - start.time;

        metrics.collectCallMetrics(CallMetrics(
                callType = "unary",
                method = (metadata?.get("rpc") ?: "None") as String,
                responseTime = timeElapsed.toDouble(),
                statusCode = status?.code.toString())
        )

        super.close(status, trailers)
    }
}