package weather.service
import cityinformation.Cityinformation.SearchRequest

interface IWeatherProvider {

    fun currentTemperature(request: SearchRequest): Double
}