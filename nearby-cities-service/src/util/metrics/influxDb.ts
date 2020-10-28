import { MetricsProvider } from "./metricsProvider";
import { CallMetrics } from "./callMetrics";
import { InfluxDB, IPoint } from 'influx';
import { ServerMetrics } from "./serverMetrics";

export default class InfluxDBMetrics implements MetricsProvider {

    private metricsWritter: (data: IPoint) => Promise<void>

    constructor(private crendentials: any) {
        const client = new InfluxDB({
            host: crendentials.host,
            username: crendentials.username,
            password: crendentials.password,
            database: crendentials.database,
        });

        this.metricsWritter = (data: IPoint) => client.writePoints([data]);
    }
    collectServerMetrics(metrics: ServerMetrics): Promise<void> {
        return this.metricsWritter(
            {
                measurement: 'perf',
                tags: { instance: process.env.HOSTNAME || 'local', service: 'nearby-cities' },
                fields: {
                    memory_usage: metrics.memoryUsage,
                    cpu_usage: metrics.cpuUsage,
                    memory_free: metrics.memoryFree
                },
            }
        );
    }

    collectCallMetrics(metrics: CallMetrics) {
        return this.metricsWritter(
            {
                measurement: 'call_data',
                tags: { instance: process.env.HOSTNAME || 'local', service: 'nearby-cities' },
                fields: { duration: metrics.responseTime, status: metrics.statusCode },
            }
        );
    }

}