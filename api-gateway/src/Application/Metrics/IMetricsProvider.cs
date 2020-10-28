using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Metrics
{
    public interface IMetricsProvider
    {
        Task CollectCallMetrics(CallData data);
        Task CollectServerMetrics(ServerData data);
    }
}
