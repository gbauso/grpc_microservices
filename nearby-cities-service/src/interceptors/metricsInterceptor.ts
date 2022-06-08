import { Interceptor } from "./interceptor";
import { inject, singleton } from 'tsyringe';
import { MetricsProvider } from "../util/metrics/metricsProvider";
import { status } from "@grpc/grpc-js";
import { $enum } from "ts-enum-util";

@singleton()
export class MetricsInterceptor implements Interceptor {

    constructor(@inject('Metrics') private metrics: MetricsProvider) { }

    intercept = async (ctx: any, next: any, error: any) => {
        
        const start = Date.now();
        const metadata = ctx.call.metadata.internalRepr;

        const rpc = metadata?.rpc ? metadata.rpc[0] : '';

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