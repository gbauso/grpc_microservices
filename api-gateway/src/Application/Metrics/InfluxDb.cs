using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using RestSharp.Extensions;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Application.Metrics
{
    public class InfluxDb : IMetricsProvider
    {
        private readonly Func<PointData, Task> _MetricsWritter;

        public InfluxDb(IOptions<MetricsConfiguration> metricsConfig)
        {
            var configuration = metricsConfig.Value;
            InfluxDBClient client;
            if (configuration.Token.HasValue())
            {
                client = InfluxDBClientFactory.Create(configuration.Host,
                                                      configuration.Token.ToCharArray());
            }
            else
            {
                client = InfluxDBClientFactory.Create(configuration.Host,
                                                      configuration.Username,
                                                      configuration.Password.ToCharArray());
            }

            _MetricsWritter = (data) => client.GetWriteApiAsync().WritePointAsync(data);
        }

        public Task CollectCallMetrics(CallData data)
        {
            var point = PointData.Measurement("call_data")
                    .Tag("call_type", data.CallType)
                      .Tag("method", data.Method)
                      .Tag("service", data.Service)
                      .Tag("instance", data.Instance)
                      .Field("status", data.Status)
                      .Field("duration", data.Duration);

            return _MetricsWritter(point);
        }

        public Task CollectServerMetrics(ServerData data)
        {
            var point = PointData.Measurement("perf")
                      .Tag("service", data.Service)
                      .Tag("instance", data.Instance)
                      .Field("cpu_usage", data.CpuUsage)
                      .Field("memory_usage", data.MemoryUsage)
                      .Field("memory_free", data.MemoryFree);


            return _MetricsWritter(point);
        }
    }
}
