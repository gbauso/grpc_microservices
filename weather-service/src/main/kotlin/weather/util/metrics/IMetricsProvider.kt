package weather.util.metrics

interface IMetricsProvider {
    fun collectCallMetrics(metrics: CallMetrics)
}