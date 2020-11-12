using Metrics.Model;
using Metrics.Providers;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Application.Metrics
{
    internal class ServerMetricsCollector
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _freeRamCounter;
        private readonly PerformanceCounter _usedRamCounter;
        private readonly IMetricsProvider _metricsProvider;

        public ServerMetricsCollector(IMetricsProvider metricsProvider)
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _freeRamCounter = new PerformanceCounter("Memory", "Available MBytes");
            _usedRamCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            _metricsProvider = metricsProvider;
        }

        private ServerData GetMetrics()
        {
            return new ServerData
            {
                CpuUsage = _cpuCounter.NextValue(),
                MemoryFree = _freeRamCounter.NextValue(),
                MemoryUsage = _usedRamCounter.NextValue()
            };
        }

        public async Task CollectServerMetrics(object? state)
        {
            await _metricsProvider.CollectServerMetrics(GetMetrics());
        }

    }
}
