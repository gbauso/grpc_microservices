package weather.service

import cityinformation.CityServiceGrpcKt
import cityinformation.Cityinformation
import org.koin.core.KoinComponent
import org.koin.core.inject

class WeatherService() : CityServiceGrpcKt.CityServiceCoroutineImplBase(), KoinComponent {

    val weatherProvider : IWeatherProvider by inject()

    override suspend fun getCityInformation(request: Cityinformation.SearchRequest) = Cityinformation.SearchResponse
            .newBuilder()
            .setWeather(weatherProvider.currentTemperature(request).toString())
            .build()
}