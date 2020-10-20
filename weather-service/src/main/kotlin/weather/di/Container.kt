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
import weather.util.metrics.InfluxDbMetrics


val openWeatherModule = module(override = true) {
    single<IWeatherProvider> { OpenWeatherProvider() }
    single<FluentLogger> { FluentLogger.getLogger("weather",
                                                    System.getenv("LOGGER_HOST"),
                                                    System.getenv("LOGGER_PORT").toInt())
                         }
    single<ILogger> { FluentdLogger()}
    single<IRegisterService> {RabbitMQRegister()}
    single<IMetricsProvider> {InfluxDbMetrics()}
}