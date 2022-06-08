package weather.interceptors

import io.grpc.ForwardingServerCall
import io.grpc.Metadata
import io.grpc.ServerCall
import io.grpc.Status
import weather.util.logging.ILogger

class ForwardingServerCall<ReqT, RespT>(delegate: ServerCall<ReqT, RespT>?, private var logger: ILogger, private var headers: Metadata) : ForwardingServerCall.SimpleForwardingServerCall<ReqT, RespT>(delegate) {
    override fun close(status: Status?, trailers: Metadata?) {
        val metadata = headers.asMap()
        if(status?.isOk!!) {
            logger.info(String.format("Request for %s FINISHED. correlation-id: %s", metadata.get("rpc"), metadata.get("correlation_id")),
                    metadata as MutableMap<String, Any>)
        }
        else {
            var map = HashMap<String, Any>()
            map.put("error", status.cause.toString())

            logger.error(String.format("Request for %s FAILED. correlation-id: %s", metadata.get("rpc"), metadata.get("correlation_id")), map)
        }


        super.close(status, trailers)
    }
}