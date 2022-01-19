package weather.di

import org.koin.dsl.module
import weather.service.OpenWeatherProvider
import weather.service.IWeatherProvider
import weather.util.logging.FileLogger
import weather.util.logging.ILogger
import weather.util.metrics.IMetricsProvider
import weather.util.metrics.Prometheus
import weather.util.secrets.DotEnvProvider
import weather.util.secrets.SystemEnvProvider
import java.io.File


val openWeatherModule = module(override = true) {
    val envProvider = if(File(".env").exists()) DotEnvProvider() else SystemEnvProvider()

    single<IWeatherProvider> { OpenWeatherProvider() }

    single<ILogger> { FileLogger() }
    single<IMetricsProvider> { Prometheus() }
    single { envProvider }
}