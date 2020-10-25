package weather

import io.grpc.Server
import io.grpc.ServerBuilder
import org.koin.core.KoinComponent
import org.koin.core.context.startKoin
import org.koin.core.inject
import weather.di.openWeatherModule
import weather.discovery.IRegisterService
import weather.interceptors.LoggingInterceptor
import weather.interceptors.MetricsInterceptor
import weather.service.WeatherService
import weather.util.logging.ILogger
import weather.util.metrics.IMetricsProvider
import weather.util.metrics.ServerMetrics
import weather.util.metrics.ServerMetricsCollector
import java.util.*
import kotlin.concurrent.schedule


class WeatherServer constructor(
        private val port: Int
) : KoinComponent {

    val logger: ILogger by inject()
    val metricsProvider: IMetricsProvider by inject()
    val register: IRegisterService by inject()
    val metricsCollector: ServerMetricsCollector by inject()

    val server: Server = ServerBuilder
            .forPort(port)
            .addService(WeatherService())
            .intercept(LoggingInterceptor(logger))
            .intercept(MetricsInterceptor(metricsProvider))
            .build()

    fun start() {
        server.start()
        logger.info("Server started, listening on $port")
        register.register(server.immutableServices.map { it.serviceDescriptor.name });
        Runtime.getRuntime().addShutdownHook(
                Thread {
                    logger.info("*** shutting down gRPC server since JVM is shutting down")
                    this@WeatherServer.stop()
                    logger.info("*** server shut down")
                }
        )
        Timer().scheduleAtFixedRate(CollectServicerMetrics(metricsProvider, metricsCollector), 0L, 5000L)
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

    val port = System.getenv("PORT").toInt()
    val server = WeatherServer(port)
    server.start()
    server.blockUntilShutdown()
}

class CollectServicerMetrics(private var metricsProvider: IMetricsProvider,
                             private var metricsCollector: ServerMetricsCollector) : TimerTask() {
    override fun run() {
        metricsProvider.collectServerMetrics(ServerMetrics(
                memoryUsage = metricsCollector.getUsedMemory(),
                memoryFree = metricsCollector.getFreeMemory()))
    }
}