package weather.discovery

import com.google.gson.Gson
import com.rabbitmq.client.ConnectionFactory
import java.lang.StringBuilder
import java.nio.charset.StandardCharsets


class RabbitMQRegister : IRegisterService {

    val QUEUE_NAME: String = "discovery"

    override fun register(handlers: List<String>) {
        val factory = ConnectionFactory()
        val port = System.getenv("SB_PORT").toInt()
        val username = System.getenv("SB_USER")
        val password = System.getenv("SB_PWD")
        val host = System.getenv("SB_HOST")

        val connectionString = String().format("amqp://%s:%s@%s:%s", username, password, host, port);

        factory.newConnection(connectionString).use { connection ->
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