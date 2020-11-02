namespace Metrics.Model
{
    public class CallData : MetricsBase
    {
        public double Duration { get; set; }
        public string Status { get; set; }
        public string CallType { get; set; }
        public string Method { get; set; }
    }
}
