using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System;
using System.Threading.Tasks;
using Metrics.Model;

namespace Metrics.Providers
{
    public class InfluxDb : IMetricsProvider
    {
        private readonly Func<PointData, Task> _MetricsWritter;
        private readonly string Service;

        public InfluxDb(MetricsConfiguration configuration, string service)
        {
            InfluxDBClient client = InfluxDBClientFactory.Create(configuration.Host,
                                                      configuration.Token.ToCharArray());

            _MetricsWritter = (data) => client.GetWriteApiAsync().WritePointAsync(configuration.Database,
                                                                                  configuration.Username,
                                                                                  data);
            Service = service;
        }

        public Task CollectCallMetrics(CallData data)
        {
            var point = PointData.Measurement("call_data")
                    .Tag("call_type", data.CallType)
                      .Tag("method", data.Method)
                      .Tag("service", Service)
                      .Tag("instance", data.Instance)
                      .Field("status", data.Status)
                      .Field("duration", data.Duration)
                      .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            return _MetricsWritter(point);
        }

        public Task CollectServerMetrics(ServerData data)
        {
            var point = PointData.Measurement("perf")
                      .Tag("service", Service)
                      .Tag("instance", data.Instance)
                      .Field("cpu_usage", data.CpuUsage)
                      .Field("memory_usage", data.MemoryUsage)
                      .Field("memory_free", data.MemoryFree)
                      .Timestamp(DateTime.UtcNow, WritePrecision.Ns);


            return _MetricsWritter(point);
        }
    }
}
