package weather.util.logging

import org.fluentd.logger.FluentLogger
import org.koin.core.KoinComponent
import org.koin.core.inject

class FluentdLogger : ILogger, KoinComponent {

    val logger: FluentLogger by inject()

    override fun debug(message: String, data: MutableMap<String, Any>) {
        log("debug", message, data)
    }

    override fun info(message: String, data: MutableMap<String, Any>) {
        log("information", message, data)
    }

    override fun error(message: String, data: MutableMap<String, Any>) {
        log("error", message, data)
    }

    private fun log(level: String, message: String, data: MutableMap<String, Any>) {
        data.put("m", message)
        data.put("Level", level)
        logger.log("", data)
    }
}

