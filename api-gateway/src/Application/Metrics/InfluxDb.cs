using InfluxDB.Client;
using InfluxDB.Client.Writes;
using System;
using System.Threading.Tasks;

namespace Application.Metrics
{
    public class InfluxDb : IMetricsProvider
    {
        private readonly InfluxDBClient _client;

        public InfluxDb(InfluxDBClient client)
        {
            _client = client;
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

            return _client.GetWriteApiAsync().WritePointAsync(point);
        }
    }
}
