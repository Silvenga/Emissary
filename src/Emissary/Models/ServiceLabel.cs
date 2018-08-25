using System.Collections.Generic;

using CommandLine;

namespace Emissary.Models
{
    public class ServiceLabel
    {
        [Value(0, Required = true)]
        public string ServiceName { get; set; }

        [Value(1, Required = true)]
        public int ServicePort { get; set; }

        [Option("tags", Separator = ',')]
        public IReadOnlyList<string> ServiceTags { get; set; }
    }
}