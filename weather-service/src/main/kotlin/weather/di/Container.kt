package weather.di

import org.koin.dsl.module
import weather.service.OpenWeatherProvider
import weather.service.IWeatherProvider
import org.fluentd.logger.FluentLogger
import org.influxdb.InfluxDB
import org.influxdb.InfluxDBFactory
import weather.discovery.IRegisterService
import weather.discovery.RabbitMQRegister
import weather.util.logging.FluentdLogger
import weather.util.logging.ILogger
import weather.util.metrics.IMetricsProvider
import weather.util.metrics.InfluxDbMetrics
import weather.util.metrics.ServerMetricsCollector


val openWeatherModule = module(override = true) {
    single<IWeatherProvider> { OpenWeatherProvider() }
    single<FluentLogger> { FluentLogger.getLogger("weather",
                                                    System.getenv("LOGGER_HOST"),
                                                    System.getenv("LOGGER_PORT").toInt())
                         }
    single<InfluxDB> { InfluxDBFactory.connect(System.getenv("METRICS_HOST_URL"),
                                                System.getenv("METRICS_USERNAME"),
                                                System.getenv("METRICS_PASSWORD"))
    }
    single<ServerMetricsCollector> { ServerMetricsCollector() }

    single<ILogger> { FluentdLogger()}
    single<IRegisterService> {RabbitMQRegister()}
    single<IMetricsProvider> {InfluxDbMetrics()}
}