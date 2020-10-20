package weather.interceptors
import io.grpc.*

fun Metadata.asMap(): Map<String, Any> {
    val metadataMap =  this.keys().map { it as String to this.get(Metadata.Key.of(it, Metadata.ASCII_STRING_MARSHALLER)) as Any }

    return metadataMap.toMap()
}



