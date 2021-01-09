package weather.discovery

import com.google.gson.Gson
import com.rabbitmq.client.ConnectionFactory
import java.lang.StringBuilder
import java.nio.charset.StandardCharsets


class RabbitMQRegister : IRegisterService, KoinComponent {

    val secrets: ISecretsProvider by inject()

    val QUEUE_NAME: String = "discovery"

    override fun register(handlers: List<String>) {
        val factory = ConnectionFactory()
        factory.port = secrets.getValue("SB_PORT").toInt()
        factory.username = secrets.getValue("SB_USER")
        factory.password = secrets.getValue("SB_PWD")
        factory.host = secrets.getValue("SB_HOST")

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