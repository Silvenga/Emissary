using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using NLog;

namespace Emissary.Core
{
    public class ContainerRegistrar
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<string, ContainerService> _containerServices = new ConcurrentDictionary<string, ContainerService>();

        public void DisoverContainers(IReadOnlyCollection<ContainerService> services)
        {
            foreach (var service in services)
            {
                var added = _containerServices.TryAdd(service.ContainerId, service);
                if (added)
                {
                    Logger.Info($"Discovered new service [{service.ServiceName}] for container [{service.ContainerId}].");
                }
            }

            foreach (var missingContainerId in _containerServices.Keys.Except(services.Select(x => x.ContainerId)))
            {
                var removed = _containerServices.TryRemove(missingContainerId, out var service);
                if (removed)
                {
                    Logger.Info($"Discovered removed service [{service.ServiceName}] for container [{service.ContainerId}].");
                }
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