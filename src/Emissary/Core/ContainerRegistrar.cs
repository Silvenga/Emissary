using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Emissary.Discovery;

namespace Emissary.Core
{
    public class ContainerRegistrar
    {
        private readonly ConcurrentDictionary<string, ContainerService> _containers = new ConcurrentDictionary<string, ContainerService>();

        public void DisoveryContainers(IReadOnlyCollection<ContainerService> containers)
        {


            foreach (var container in containers)
            {
                //_containers.TryAdd(container.Id,);
            }
        }
    }

    public class ContainerService
    {
        public string ContainerId { get; set; }

        public string ServiceName { get; set; }

        public int ServicePort { get; set; }

        public IReadOnlyList<string> ServiceTags { get; set; }
    }
}