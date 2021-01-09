package weather.util.secrets

interface ISecretProvider {
    fun getValue(key: String): String
}