package weather.di

import com.influxdb.client.InfluxDBClient
import com.influxdb.client.InfluxDBClientFactory
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
import weather.util.metrics.ServerMetricsCollector


val openWeatherModule = module(override = true) {
    single<IWeatherProvider> { OpenWeatherProvider() }
    single<FluentLogger> {
        FluentLogger.getLogger("weather",
                System.getenv("LOGGER_HOST"),
                System.getenv("LOGGER_PORT").toInt())
    }
    single<InfluxDBClient> {
        if (System.getenv("METRICS_TOKEN") == null)
            InfluxDBClientFactory.createV1(System.getenv("METRICS_HOST_URL"),
                    System.getenv("METRICS_USERNAME"),
                    System.getenv("METRICS_PASSWORD").toCharArray(),
                    "metrics", null)
        else
            InfluxDBClientFactory.create(System.getenv("METRICS_HOST_URL"),
                    System.getenv("METRICS_TOKEN").toCharArray(),
                    System.getenv("METRICS_USERNAME"),
                    "metrics")
    }
    single<ServerMetricsCollector> { ServerMetricsCollector() }

    single<ILogger> { FluentdLogger() }
    single<IRegisterService> { RabbitMQRegister() }
    single<IMetricsProvider> { InfluxDbMetrics() }
}