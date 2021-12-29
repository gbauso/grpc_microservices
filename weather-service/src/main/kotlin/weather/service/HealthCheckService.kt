package weather.service

import io.grpc.health.v1.HealthGrpc
import io.grpc.health.v1.HealthCheckRequest
import io.grpc.health.v1.HealthCheckResponse
import io.grpc.stub.StreamObserver
import io.grpc.health.v1.HealthCheckResponse.ServingStatus;

class HealthCheckService() : HealthGrpc.HealthImplBase() {

    var serving: Boolean = true

    override open fun watch(request: HealthCheckRequest, responseObserver: StreamObserver<HealthCheckResponse>) {
        val status = if (serving == true) ServingStatus.SERVING else ServingStatus.NOT_SERVING
        
        responseObserver.onNext(
            HealthCheckResponse
                .newBuilder()
                .setStatus(status)
                .build()
        )

        responseObserver.onCompleted()
    }

    fun onExit() {
        serving = false
    }
}