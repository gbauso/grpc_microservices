package weather.interceptors

import io.grpc.*
import weather.util.metrics.IMetricsProvider
import java.util.*

class MetricsInterceptor(var metrics: IMetricsProvider) : ServerInterceptor {

    override fun <ReqT : Any?, RespT : Any?> interceptCall(call: ServerCall<ReqT, RespT>?, headers: Metadata?, next: ServerCallHandler<ReqT, RespT>?): ServerCall.Listener<ReqT>? {
        val current = Date()

        return next?.startCall(MetricsForwardingServerCall(call, metrics, current, headers), headers)
    }
}




