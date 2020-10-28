using System.Diagnostics;

namespace Application.Metrics
{
    public class ServerMetricsCollector
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _freeRamCounter;
        private readonly PerformanceCounter _usedRamCounter;

        public ServerMetricsCollector()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _freeRamCounter = new PerformanceCounter("Memory", "Available MBytes");
            _usedRamCounter = new PerformanceCounter("Memory", "FreePhysicalMemory");
        }

        public ServerData GetMetrics()
        {
            return new ServerData
            {
                CpuUsage = _cpuCounter.NextValue(),
                MemoryFree = _freeRamCounter.NextValue(),
                MemoryUsage = _usedRamCounter.NextValue()
            };
        }
    }
}
