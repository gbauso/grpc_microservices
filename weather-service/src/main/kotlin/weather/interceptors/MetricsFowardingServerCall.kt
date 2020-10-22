package weather.interceptors

import io.grpc.ForwardingServerCall
import io.grpc.Metadata
import io.grpc.ServerCall
import io.grpc.Status
import weather.util.metrics.CallMetrics
import weather.util.metrics.IMetricsProvider
import java.util.*

class MetricsForwardingServerCall<ReqT, RespT>(delegate: ServerCall<ReqT, RespT>?, private var metrics: IMetricsProvider, private var start: Date) : ForwardingServerCall.SimpleForwardingServerCall<ReqT, RespT>(delegate) {
    override fun close(status: Status?, trailers: Metadata?) {

        val timeElapsed = Date().time - start.time;

        metrics.collectCallMetrics(CallMetrics(
                callType = "unary",
                method = "any",
                responseTime = timeElapsed,
                statusCode = status.toString())
        )

        super.close(status, trailers)
    }
}