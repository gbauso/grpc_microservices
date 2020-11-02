namespace Metrics
{
    public class MetricsConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Database { get; set; }
        public string Token { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Host)
                   && !string.IsNullOrEmpty(Username)
                   && !string.IsNullOrEmpty(Database);
        }
    }
}
