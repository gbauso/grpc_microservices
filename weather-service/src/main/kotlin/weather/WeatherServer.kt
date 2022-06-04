package weather

import io.grpc.Server
import io.grpc.ServerBuilder
import io.grpc.health.v1.HealthCheckResponse
import io.grpc.protobuf.services.HealthStatusManager
import io.grpc.protobuf.services.ProtoReflectionService
import org.koin.core.KoinComponent
import org.koin.core.context.startKoin
import org.koin.core.inject
import weather.di.openWeatherModule
import weather.interceptors.LoggingInterceptor
import weather.interceptors.MetricsInterceptor
import weather.service.WeatherService
import weather.util.logging.ILogger
import weather.util.metrics.IMetricsProvider
import weather.util.secrets.ISecretProvider


class WeatherServer constructor(
) : KoinComponent {

    val logger: ILogger by inject()
    val metricsProvider: IMetricsProvider by inject()
    val secrets: ISecretProvider by inject()
    val port = secrets.getValue("PORT").toInt()
    var healthStatusManager = HealthStatusManager()

    val server: Server = ServerBuilder
            .forPort(port)
            .addService(WeatherService())
            .addService(healthStatusManager.healthService)
            .addService(ProtoReflectionService.newInstance())
            .intercept(LoggingInterceptor(logger))
            .intercept(MetricsInterceptor(metricsProvider))
            .build()

    fun start() {
        healthStatusManager.setStatus("cityinformation.CityService", HealthCheckResponse.ServingStatus.SERVING)
        healthStatusManager.setStatus("grpc.health.v1.Health", HealthCheckResponse.ServingStatus.SERVING)
        healthStatusManager.setStatus("grpc.reflection.v1alpha.ServerReflection", HealthCheckResponse.ServingStatus.SERVING)
        
        server.start()
        logger.info("Server started, listening on $port", mutableMapOf("port" to (port as Any)))
        Runtime.getRuntime().addShutdownHook(
                Thread {
                    logger.info("*** shutting down gRPC server since JVM is shutting down")
                    this@WeatherServer.stop()
                    healthStatusManager.enterTerminalState()
                    logger.info("*** server shut down")
                }
        )
    }

    private fun stop() {
        server.shutdown()
    }

    fun blockUntilShutdown() {
        server.awaitTermination()
    }
}



fun main() {
    startKoin {
        modules(openWeatherModule)
    }

    val server = WeatherServer()
    server.start()
    server.blockUntilShutdown()
}
