using Newtonsoft.Json;
using System.Collections.Generic;

namespace DiscoveryService
{
    public class Discovery
    {
        public Discovery()
        {
            Handlers = new List<string>();
        }

        public string Service { get; set; }
        public IEnumerable<string> Handlers { get; set; }
    }
}
