package weather.util.metrics

import org.influxdb.InfluxDB
import org.influxdb.dto.Point
import org.koin.core.KoinComponent
import org.koin.core.inject

public class InfluxDbMetrics: IMetricsProvider, KoinComponent  {
    val client: InfluxDB by inject()

    constructor() {
        client.createDatabase("weather_metrics")
        client.setDatabase("weather_metrics")
    }

    override fun collectCallMetrics(metrics: CallMetrics) {
        with(client) {
            write(Point.measurement("call_data")
                .tag("callType", metrics.callType)
                .tag("method", metrics.method)
                .addField("status", metrics.statusCode)
                .addField("duration", metrics.responseTime)
                .build())
        }
    }

    override fun collectServerMetrics(metrics: ServerMetrics) {
        with(client) {
            write(Point.measurement("perf")
                    .addField("cpu_usage", metrics.cpuUsage)
                    .addField("memory_usage", metrics.memoryFree)
                    .addField("memory_free", metrics.memoryFree)
                    .build())
        }
    }
}