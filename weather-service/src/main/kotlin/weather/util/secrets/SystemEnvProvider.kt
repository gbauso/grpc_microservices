package weather.util.secrets

class SystemEnvProvider(): ISecretProvider {

    override fun getValue(key: String): String = System.getenv(key)
}

