package weather.util

import org.fluentd.logger.FluentLogger
import org.koin.core.KoinComponent
import org.koin.core.inject

class FluentdLogger : ILogger, KoinComponent {

    val logger: FluentLogger by inject()

    override fun debug(message: String, data: MutableMap<String, Any>) {
        log("Debug", message, data)
    }

    override fun info(message: String, data: MutableMap<String, Any>) {
        log("Information", message, data)
    }

    override fun error(message: String, data: MutableMap<String, Any>) {
        log("Error", message, data)
    }

    private fun log(level: String, message: String, data: MutableMap<String, Any>) {
        data.put("m", message)
        data.put("level", level)
        logger.log("", data)
    }
}

