package weather

import io.grpc.Server
import io.grpc.ServerBuilder
import org.koin.core.KoinComponent
import org.koin.core.context.startKoin
import org.koin.core.inject
import weather.di.openWeatherModule
import weather.discovery.IRegisterService
import weather.interceptors.LoggingInterceptor
import weather.service.WeatherService
import weather.util.ILogger

class WeatherServer constructor(
        private val port: Int
) : KoinComponent  {

    val logger: ILogger by inject()
    val register: IRegisterService by inject()

    val server: Server = ServerBuilder
            .forPort(port)
            .addService(WeatherService())
            .intercept(LoggingInterceptor(logger))
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