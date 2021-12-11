package weather

import io.grpc.Server
import io.grpc.ServerBuilder
import org.koin.core.KoinComponent
import org.koin.core.context.startKoin
import org.koin.core.inject
import weather.di.openWeatherModule
import weather.interceptors.LoggingInterceptor
import weather.service.HealthCheckService
import weather.interceptors.MetricsInterceptor
import weather.service.WeatherService
import weather.util.logging.ILogger
import weather.util.metrics.IMetricsProvider
import weather.util.secrets.ISecretProvider
import java.util.*
import kotlin.concurrent.schedule


class WeatherServer constructor(
) : KoinComponent {

    val logger: ILogger by inject()
    val metricsProvider: IMetricsProvider by inject()
    val secrets: ISecretProvider by inject()
    val port = secrets.getValue("PORT").toInt()

    val server: Server = ServerBuilder
            .forPort(port)
            .addService(WeatherService())
            .addService(HealthCheckService())
            .intercept(LoggingInterceptor(logger))
            .intercept(MetricsInterceptor(metricsProvider))
            .build()

    fun start() {
        server.start()
        logger.info("Server started, listening on $port")
        Runtime.getRuntime().addShutdownHook(
                Thread {
                    logger.info("*** shutting down gRPC server since JVM is shutting down")
                    this@WeatherServer.stop()
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
