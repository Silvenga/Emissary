using System;
using System.Collections.Generic;

namespace Emissary.Models
{
    public class ContainerService
    {
        public string ContainerId { get; set; }

        public string ServiceName { get; set; }

        public int ServicePort { get; set; }

        public IReadOnlyList<string> ServiceTags { get; set; }

        public string ContainerStatus { get; set; }

        public DateTime ContainerCreationOn { get; set; }
    }
}