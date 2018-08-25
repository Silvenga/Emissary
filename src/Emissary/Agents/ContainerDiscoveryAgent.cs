using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Emissary.Core;
using Emissary.Discovery;

using NLog;

namespace Emissary.Agents
{
    public class ContainerDiscoveryAgent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly JobScheduler _scheduler;
        private readonly ContainerDiscoveryClient _client;

        public ContainerDiscoveryAgent(JobScheduler scheduler, ContainerDiscoveryClient client)
        {
            _scheduler = scheduler;
            _client = client;
        }

        public void Monitor(ContainerRegistrar registrar, CancellationToken token)
        {
            _scheduler.ScheduleRecurring<ContainerDiscoveryAgent>(() => Poll(registrar, token), token);
        }

        private async Task Poll(ContainerRegistrar registrar, CancellationToken token)
        {
            var desiredContainers = await _client.GetContainerServices(token);

            registrar.Operate(transation =>
            {
                var currentContainers = transation.GetContainers();

                var newContainers = from containerId in desiredContainers.Select(x => x.ContainerId).Except(currentContainers).Distinct()
                                    from containerService in desiredContainers.Where(x => x.ContainerId == containerId)
                                    select containerService;
                var extraContainers = currentContainers.Except(desiredContainers.Select(x => x.ContainerId)).Distinct();

                foreach (var service in newContainers)
                {
                    Logger.Info($"Discovered service [{service.ServiceName}] under container [{service.ContainerId}].");
                    transation.AddContainerService(service);
                }

                foreach (var containerId in extraContainers)
                {
                    Logger.Info($"Container [{containerId}] was removed.");
                    transation.DeleteContainer(containerId);
                }
            });
        }
    }
}