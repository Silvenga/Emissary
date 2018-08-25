using System;
using System.Collections.Generic;

namespace Emissary.Discovery
{
    public class DiscoveredContainer
    {
        public string Id { get; set; }
        public IDictionary<string, string> Labels { get; set; }
        public DateTime Created { get; set; }
        public IList<string> Names { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
    }
}