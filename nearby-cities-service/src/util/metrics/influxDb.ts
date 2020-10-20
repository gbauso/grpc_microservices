import { MetricsProvider } from "./metricsProvider";
import { CallMetrics } from "./callMetrics";
import { InfluxDB, FieldType, IPoint } from 'influx';
import { ServerMetrics } from "./serverMetrics";
import { singleton } from "tsyringe";

@singleton()
export class InfluxDBMetrics implements MetricsProvider {

    private client: InfluxDB

    constructor() {
        this.client = new InfluxDB({
            host: 'localhost',
            username: 'admin',
            password: 'password123',
            database: 'nearby_cities_metrics',
            schema: [
                {
                    measurement: 'call_data',
                    fields: {
                        status: FieldType.STRING,
                        duration: FieldType.INTEGER
                    },
                    tags: [
                        'method',
                        'callType'
                    ]
                },
                {
                    measurement: 'perf',
                    tags: ['hostname'],
                    fields: {
                      memory_usage: FieldType.INTEGER,
                      cpu_usage: FieldType.FLOAT,
                      memory_free: FieldType.INTEGER,
                    }
                }
            ]
        });

        this.client.createDatabase('nearby_cities_metrics').then();
    }
    collectServerMetrics(metrics: ServerMetrics): Promise<void> {
        return this.client.writePoints([
            {
                measurement: 'perf',
                tags: { hostname: process.env.REGISTER_AS as string },
                fields: { 
                    memory_usage: metrics.memoryUsage,
                    cpu_usage: metrics.cpuUsage,
                    memory_free: metrics.memoryFree
                 },
            }
        ]
        );
    }

    collectCallMetrics(metrics: CallMetrics) {
        return this.client.writePoints([
            {
                measurement: 'call_data',
                tags: { method: metrics.method, callType: metrics.callType },
                fields: { duration: metrics.responseTime, status: metrics.statusCode },
            }
        ]
        );
    }

}