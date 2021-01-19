import { CallMetrics } from "./callMetrics";

export interface MetricsProvider {
    collectCallMetrics(metrics: CallMetrics) : void
}