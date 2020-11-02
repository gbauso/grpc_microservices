namespace Metrics.Model
{
    public class ServerData : MetricsBase
    {
        public float CpuUsage { get; set; } 
        public float MemoryUsage { get; set; }
        public float MemoryFree { get; set; }
    }
}
