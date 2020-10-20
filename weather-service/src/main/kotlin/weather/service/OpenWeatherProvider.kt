package weather.service

import cityinformation.Cityinformation
import khttp.get;

public class OpenWeatherProvider : IWeatherProvider {

    override fun currentTemperature(request: Cityinformation.SearchRequest) =
            get(String.format("https://api.openweathermap.org/data/2.5/weather?lat=%s&lon=%s&appid=%s&units=metric",
                request.lat, request.lon, System.getenv("OPENWEATHER_APP_ID")))
                    .jsonObject.getJSONObject("main").getDouble("temp")

}