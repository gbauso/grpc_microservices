package weather.util.logging

import mu.KotlinLogging
import org.koin.core.KoinComponent
import org.koin.core.inject

class FileLogger : ILogger, KoinComponent {

    private val logger = KotlinLogging.logger {} 

    override fun debug(message: String, data: MutableMap<String, Any>) {
        logger.debug(message, data) 
    }

    override fun info(message: String, data: MutableMap<String, Any>) {
        logger.info(message, data)
    }

    override fun error(message: String, data: MutableMap<String, Any>) {
        logger.error(message, data)
    }
}

