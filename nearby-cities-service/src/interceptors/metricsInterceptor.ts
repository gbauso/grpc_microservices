import { Interceptor } from "./interceptor";
import { inject, singleton } from 'tsyringe';
import { MetricsProvider } from "../util/metrics/metricsProvider";
import { status } from "grpc";
import { $enum } from "ts-enum-util";

@singleton()
export class MetricsInterceptor implements Interceptor {

    constructor(@inject('Metrics') private metrics: MetricsProvider) { }

    intercept = async (ctx: any, next: any) => {
        
        const start = Date.now();
        const metadata = ctx.call.metadata._internal_repr;

        const rpc = metadata?.rpc?.get(0) || 'undefined';

        await next();

        const responseTime = (Date.now() - start);
        this.metrics.collectCallMetrics({
            responseTime: responseTime,
            statusCode: $enum(status).getKeys()[ctx.status.code].toString(),
            callType: 'unary',
            method: rpc
        });
    }

}