import { CallMetrics } from "./callMetrics";
import { ServerMetrics } from "./serverMetrics";

export interface MetricsProvider {
    collectCallMetrics(metrics: CallMetrics) : Promise<void>
    collectServerMetrics(metrics: ServerMetrics) : Promise<void>
}