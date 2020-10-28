import { MetricsProvider } from "./metricsProvider";
import { CallMetrics } from "./callMetrics";
import config from '../../../config.json';
import { ServerMetrics } from "./serverMetrics";
import { singleton } from "tsyringe";
import { InfluxDB, Point } from "@influxdata/influxdb-client";


@singleton()
export default class InfluxDBV2Metrics implements MetricsProvider {

  private metricsWriter: (data: Point) => void

  constructor(private crendentials: any) {
    const token = crendentials.token;
    const username = crendentials.username;
    const host = crendentials.host;

    const client = new InfluxDB({ url: host, token: token });
    const writeApi = client.getWriteApi(username, crendentials.database);
    this.metricsWriter = (data: Point) => writeApi.writePoint(data);
  }

  collectServerMetrics(metrics: ServerMetrics): Promise<void> {
    const metric: Point = new Point("perf")
      .tag("service", "nearby-cities")
      .tag("instance", (process.env.HOSTNAME || "local"))
      .floatField("cpu_usage", metrics.cpuUsage)
      .floatField("memory_usage", metrics.memoryUsage)
      .floatField("memory_free", metrics.memoryFree)


    return Promise.resolve(this.metricsWriter(metric));
  }

  collectCallMetrics(metrics: CallMetrics) {
    const metric: Point = new Point("call_data")
      .tag("call_type", metrics.callType)
      .tag("method", metrics.method)
      .tag("service", "nearby-cities")
      .tag("instance", (process.env.HOSTNAME || "local"))
      .stringField("status", metrics.statusCode)
      .floatField("duration", metrics.responseTime)


    return Promise.resolve(this.metricsWriter(metric));
  }

}