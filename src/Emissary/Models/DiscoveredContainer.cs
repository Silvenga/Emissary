using System;
using System.Collections.Generic;

namespace Emissary.Models
{
    public class DiscoveredContainer
    {
        public string Id { get; set; }
        public IReadOnlyDictionary<string, string> Labels { get; set; }
        public DateTime Created { get; set; }
        public IReadOnlyList<int> Ports { get; set; }
        public string Names { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
        public string Images { get; set; }
    }
}