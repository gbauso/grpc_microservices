package weather.service

import healthcheck.HealthCheckServiceGrpcKt
import healthcheck.Healthcheck

class HealthCheckService() : HealthCheckServiceGrpcKt.HealthCheckServiceCoroutineImplBase() {

    override suspend fun getStatus(request: Healthcheck.Empty) = Healthcheck.PingResponse
            .newBuilder()
            .setResponse("Pong")
            .build()
    }
