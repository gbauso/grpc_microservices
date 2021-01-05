package weather.interceptors

import io.grpc.*
import weather.util.logging.ILogger

class LoggingInterceptor(val logger: ILogger) : ServerInterceptor {

    override fun <ReqT : Any?, RespT : Any?> interceptCall(call: ServerCall<ReqT, RespT>?, headers: Metadata?, next: ServerCallHandler<ReqT, RespT>?): ServerCall.Listener<ReqT>? {
        val metadata = headers?.asMap()
        logger.info(String.format("Request for %s STARTED", metadata?.get("rpc")),
                metadata as MutableMap<String, Any>)

        return next?.startCall(LoggingForwardingServerCall(call, logger, headers), headers)
    }
}




