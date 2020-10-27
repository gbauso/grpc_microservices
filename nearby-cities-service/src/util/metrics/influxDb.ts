import { MetricsProvider } from "./metricsProvider";
import { CallMetrics } from "./callMetrics";
import { InfluxDB, IPoint } from 'influx';
import config from '../../../config.json';
import { ServerMetrics } from "./serverMetrics";
import { singleton } from "tsyringe";

@singleton()
export default class InfluxDBMetrics implements MetricsProvider {

    private metricsWritter: (data: IPoint) => Promise<void>

    constructor() {
        const client = new InfluxDB({
            host: process.env.METRICS_HOSTNAME || config.metrics.host,
            username: process.env.METRICS_USERNAME || config.metrics.username,
            password: process.env.METRICS_USERNAME || config.metrics.username,
            database: config.metrics.database,
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