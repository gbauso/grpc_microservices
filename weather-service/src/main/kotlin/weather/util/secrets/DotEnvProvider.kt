package weather.util.secrets

import io.github.cdimascio.dotenv.dotenv

class DotEnvProvider: ISecretProvider {
    private val dotenv = dotenv()

    override fun getValue(key: String) = dotenv[key]
}

