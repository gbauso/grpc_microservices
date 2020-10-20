package weather.discovery

import com.google.gson.Gson
import com.rabbitmq.client.ConnectionFactory
import java.nio.charset.StandardCharsets

//import com.rabbitmq.client

class RabbitMQRegister : IRegisterService {

    val QUEUE_NAME: String = "discovery"

    override fun register(handlers: List<String>) {
        val factory = ConnectionFactory()
        factory.port = System.getenv("SB_PORT").toInt()
        factory.username = System.getenv("SB_USER")
        factory.password = System.getenv("SB_PWD")
        factory.host = System.getenv("SB_HOST")

        factory.newConnection().use { connection ->
            connection.createChannel().use { channel ->
                channel.queueDeclare(QUEUE_NAME, true, false, false, null)
                val message = Gson().toJson(RegisterMessage(handlers))
                channel.basicPublish(
                        "",
                        QUEUE_NAME,
                        null,
                        message.toByteArray(StandardCharsets.UTF_8)
                )
            }
        }
    }

}