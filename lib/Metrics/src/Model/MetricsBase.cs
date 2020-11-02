using System;

namespace Metrics.Model
{
    public abstract class MetricsBase
    {
        public string Service { get; set; }
        public string Instance = Environment.GetEnvironmentVariable("HOSTNAME") ?? "local";
    }
}
