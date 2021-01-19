package weather.service

import cityinformation.Cityinformation
import khttp.get;
import org.koin.core.KoinComponent
import org.koin.core.inject
import weather.util.secrets.ISecretProvider

public class OpenWeatherProvider : IWeatherProvider, KoinComponent {

    val secrets: ISecretProvider by inject()

    override fun currentTemperature(request: Cityinformation.SearchRequest) =
            get(String.format("https://api.openweathermap.org/data/2.5/weather?lat=%s&lon=%s&appid=%s&units=metric",
                request.lat, request.lon, secrets.getValue("OPENWEATHER_APP_ID")))
                    .jsonObject.getJSONObject("main").getDouble("temp")

}