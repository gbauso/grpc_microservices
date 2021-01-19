package weather.util.logging

interface ILogger {
    fun debug(message: String, data: MutableMap<String, Any> = HashMap())
    fun info(message: String, data: MutableMap<String, Any> = HashMap())
    fun error(message: String, data: MutableMap<String, Any> = HashMap())
}