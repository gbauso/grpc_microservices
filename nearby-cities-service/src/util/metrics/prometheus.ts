import { CallMetrics } from "./callMetrics";
import { MetricsProvider } from "./metricsProvider";
import { Counter, Histogram, collectDefaultMetrics, Registry } from 'prom-client';
import express from 'express';

type Metrics = {
    grpcServerStartedTotal: Counter<'grpc_type' | 'grpc_method'>;
    grpcServerHandledTotal: Counter<'grpc_type' | 'grpc_method' | 'grpc_code'>;
    grpcServerHandlingSeconds: Histogram<'grpc_type' | 'grpc_method' | 'grpc_code'>;
}

export default class Prometheus implements MetricsProvider {

    metrics!: Metrics;
    defaultBuckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10];

    constructor() {
        const registry = new Registry();
        collectDefaultMetrics({register: registry});
        this.configureMetrics(registry);
        this.expose(registry);
    }

    expose = (registry: Registry) => {
        const server = express();

        server.get('/metrics', (req, res) => {
            res.send(registry.metrics())
        })
        server.listen(3009);
    }

    private configureMetrics(registry: Registry) {

        this.metrics = {
            grpcServerStartedTotal: new Counter({
                labelNames: ['grpc_type', 'grpc_method'],
                name: 'grpc_server_started_total',
                help: 'Total number of RPCs started on the server.',
            }),
            grpcServerHandledTotal: new Counter({
                labelNames: ['grpc_type', 'grpc_method', 'grpc_code'],
                name: 'grpc_server_handled_total',
                help: 'Total number of RPCs completed on the server, regardless of success or failure.',
            }),
            grpcServerHandlingSeconds: new Histogram({
                labelNames: ['grpc_type', 'grpc_method', 'grpc_code'],
                name: 'grpc_server_handling_seconds',
                buckets: this.defaultBuckets,
                help: 'Histogram of response latency (seconds) of gRPC that had been application-level handled by the server.Duration of HTTP response size in bytes',
            })
        }

        registry.registerMetric(this.metrics.grpcServerHandledTotal);
        registry.registerMetric(this.metrics.grpcServerHandlingSeconds);
        registry.registerMetric(this.metrics.grpcServerStartedTotal);
    }

    collectCallMetrics(metrics: CallMetrics): void {
        this.metrics.grpcServerStartedTotal
            .labels(metrics.callType, metrics.method)
            .inc();

        this.metrics.grpcServerHandledTotal
            .labels(metrics.callType, metrics.method, metrics.statusCode)
            .inc();

        this.metrics.grpcServerHandlingSeconds
            .labels(metrics.callType, metrics.method, metrics.statusCode)
            .observe(metrics.responseTime);
    }

}