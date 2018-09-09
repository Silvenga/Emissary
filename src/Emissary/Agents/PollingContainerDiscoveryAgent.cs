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
        private readonly EmissaryConfiguration _configuration;

        public PollingContainerDiscoveryAgent(JobScheduler scheduler, ContainerDiscoveryClient client, EmissaryConfiguration configuration)
        {
            _scheduler = scheduler;
            _client = client;
            _configuration = configuration;
        }

        public async Task Monitor(IContainerRegistrar registrar, CancellationToken token)
        {
            var pollingInterval = TimeSpan.FromSeconds(int.Parse(_configuration.PollingInterval));
            await _scheduler.ScheduleRecurring<PollingContainerDiscoveryAgent>(() => Poll(registrar, token), token, pollingInterval);
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
                    Logger.Info($"Discovered services [{service.ServiceName}] for container [{service.ContainerId.ToShortContainerName()}].");
                    transaction.AddContainerService(service);
                }

                foreach (var service in updatedContainers)
                {
                    transaction.UpdateContainerService(service);
                }

                foreach (var containerId in extraContainers)
                {
                    Logger.Info($"Container [{containerId.ToShortContainerName()}] was removed.");
                    transaction.DeleteContainer(containerId);
                }
            }
        }
    }
}