using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Metrics
{
    public class ServerData : MetricsBase
    {
        public float CpuUsage { get; set; }
        public float MemoryUsage { get; set; }
        public float MemoryFree { get; set; }
    }
}
