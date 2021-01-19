package weather.di

import org.koin.dsl.module
import weather.service.OpenWeatherProvider
import weather.service.IWeatherProvider
import org.fluentd.logger.FluentLogger
import weather.discovery.IRegisterService
import weather.discovery.RabbitMQRegister
import weather.util.logging.FluentdLogger
import weather.util.logging.ILogger
import weather.util.metrics.IMetricsProvider
import weather.util.metrics.Prometheus
import weather.util.secrets.DotEnvProvider
import weather.util.secrets.SystemEnvProvider
import java.io.File


val openWeatherModule = module(override = true) {
    val envProvider = if(File(".env").exists()) DotEnvProvider() else SystemEnvProvider()

    single<IWeatherProvider> { OpenWeatherProvider() }
    single<FluentLogger> {
        FluentLogger.getLogger("weather",
                envProvider.getValue("LOGGER_HOST"),
                envProvider.getValue("LOGGER_PORT").toInt())
    }

    single<ILogger> { FluentdLogger() }
    single<IRegisterService> { RabbitMQRegister() }
    single<IMetricsProvider> { Prometheus() }
    single { envProvider }
}