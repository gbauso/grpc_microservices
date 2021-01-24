namespace Utils.Grpc.Metrics
{
    internal class CallData 
    {
        public double Duration { get; set; }
        public string Status { get; set; }
        public string CallType { get; set; }
        public string Method { get; set; }
    }
}
