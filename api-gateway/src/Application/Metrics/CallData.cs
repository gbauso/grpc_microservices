using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Metrics
{
    public class CallData : MetricsBase
    {
        public decimal Duration { get; set; }
        public string Status { get; set; }
        public string CallType { get; set; }
        public string Method { get; set; }
    }
}
