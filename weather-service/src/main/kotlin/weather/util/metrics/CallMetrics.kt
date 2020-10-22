package weather.util.metrics

class CallMetrics(
        val callType: String,
        val method: String,
        val responseTime: Long,
        val statusCode: String)
