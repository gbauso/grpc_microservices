package weather.util.metrics

import com.influxdb.client.write.Point;
import com.influxdb.client.InfluxDBClient
import org.koin.core.KoinComponent
import com.influxdb.client.domain.WritePrecision;
import org.koin.core.inject
import java.time.Instant

public class InfluxDbMetrics : IMetricsProvider, KoinComponent {
    val client: InfluxDBClient by inject()
    val bucket = "metrics"


    override fun collectCallMetrics(metrics: CallMetrics) {
        with(client.writeApi) {
            writePoint(Point.measurement("call_data")
                    .addTag("call_type", metrics.callType)
                    .addTag("method", metrics.method)
                    .addTag("service", "weather")
                    .addField("status", metrics.statusCode)
                    .addField("duration", metrics.responseTime)
                    .time(Instant.now(), WritePrecision.NS))
        }
    }

    override fun collectServerMetrics(metrics: ServerMetrics) {
        with(client.writeApi) {
            writePoint(Point.measurement("perf")
                    .addTag("service", "weather")
                    .addField("memory_usage", metrics.memoryUsage)
                    .addField("memory_free", metrics.memoryFree)
                    .time(Instant.now(), WritePrecision.NS))
        }
    }
}