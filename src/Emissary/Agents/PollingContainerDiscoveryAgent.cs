using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Emissary.Clients;
using Emissary.Core;

using NLog;

namespace Emissary.Agents
{
    public class PollingContainerDiscoveryAgent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly JobScheduler _scheduler;
        private readonly ContainerDiscoveryClient _client;

        public PollingContainerDiscoveryAgent(JobScheduler scheduler, ContainerDiscoveryClient client)
        {
            _scheduler = scheduler;
            _client = client;
        }

        public void Monitor(IContainerRegistrar registrar, CancellationToken token)
        {
            _scheduler.ScheduleRecurring<PollingContainerDiscoveryAgent>(() => Poll(registrar, token), token, TimeSpan.FromMinutes(1));
        }

        private async Task Poll(IContainerRegistrar registrar, CancellationToken token)
        {
            using (var transaction = await registrar.BeginTransaction())
            {
                var desiredContainers = await _client.GetRunningContainerServices(token);
                var desiredContainerIds = desiredContainers.Select(x => x.ContainerId).ToList();
                var currentContainers = transaction.GetContainers();

                var newContainers = from containerId in desiredContainerIds.Except(currentContainers).Distinct()
                                    from containerService in desiredContainers.Where(x => x.ContainerId == containerId)
                                    select containerService;
                var updatedContainers = from c in desiredContainers
                                        from v in currentContainers.Where(x => c.ContainerId == x)
                                        select c;
                var extraContainers = currentContainers.Except(desiredContainerIds).Distinct();

                foreach (var service in newContainers)
                {
                    Logger.Info($"Discovered services [{service.ServiceName}] for container [{service.ContainerId}].");
                    transaction.AddContainerService(service);
                }

                foreach (var service in updatedContainers)
                {
                    transaction.UpdateContainerService(service);
                }

                foreach (var containerId in extraContainers)
                {
                    Logger.Info($"Container [{containerId}] was removed.");
                    transaction.DeleteContainer(containerId);
                }
            }
        }
    }
}