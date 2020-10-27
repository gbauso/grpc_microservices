using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Metrics
{
    public abstract class MetricsBase
    {
        public string Service = "api-gateway";
        public string Instance = Environment.GetEnvironmentVariable("HOSTNAME") ?? "local";
    }
}
