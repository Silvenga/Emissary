using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Emissary.Clients;
using Emissary.Core;
using Emissary.Models;

using NLog;

namespace Emissary.Agents
{
    public class SubscribingContainerDiscoveryAgent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly JobScheduler _scheduler;
        private readonly ContainerDiscoveryClient _client;
        private readonly YieldingProgress<ContainerEvent> _yieldingProgress = new YieldingProgress<ContainerEvent>();

        public SubscribingContainerDiscoveryAgent(JobScheduler scheduler, ContainerDiscoveryClient client)
        {
            _scheduler = scheduler;
            _client = client;
        }

        public void Monitor(IContainerRegistrar registrar, CancellationToken token)
        {
            _scheduler.ScheduleRecurring<PollingContainerDiscoveryAgent>(() => Subscribe(token), token);
            _scheduler.ScheduleRecurring<PollingContainerDiscoveryAgent>(() => HandleEvents(registrar, token), token);
        }

        private async Task Subscribe(CancellationToken token)
        {
            Logger.Info("Subscribing to the Docker event stream.");
            await _client.GetEvents(_yieldingProgress, token);
        }

        private async Task HandleEvents(IContainerRegistrar registrar, CancellationToken token)
        {
            foreach (var progress in _yieldingProgress.GetEnumerable(token))
            {
                using (var transaction = await registrar.BeginTransaction())
                {
                    //Logger.Info($"Handing new event {progress.Status} for container {progress.ContainerId}.");

                    // attach, commit, copy, create, destroy, detach, die, exec_create, exec_detach, exec_start, export, health_status, kill, oom, pause, rename, resize, restart, start, stop, top, unpause, update

                    switch (progress.Status)
                    {
                        case "update":
                        case "start":
                        case "restart":
                            var containerServices = await _client.GetContainerService(progress.ContainerId, token);
                            foreach (var containerService in containerServices)
                            {
                                ConsiderAddedService(transaction, containerService);
                            }
                            break;
                        case "destroy":
                        case "die":
                        case "stop":
                        case "kill":
                            ConsiderRemovedContainer(transaction, progress.ContainerId);
                            break;
                    }
                }
            }
        }

        private void ConsiderAddedService(IContainerRegistrarTransaction transaction, ContainerService service)
        {
            var exists = transaction.ContainerServiceExists(service.ContainerId, service.ServiceName);
            if (exists)
            {
                transaction.UpdateContainerService(service);
            }
            else
            {
                Logger.Info($"Recieved creation event of services [{service.ServiceName}] for container [{service.ContainerId.ToShortContainerName()}].");
                transaction.AddContainerService(service);
            }
        }

        private void ConsiderRemovedContainer(IContainerRegistrarTransaction transaction, string containerId)
        {
            var exists = transaction.GetContainers().Contains(containerId);
            if (exists)
            {
                Logger.Info($"Recieved event that container [{containerId.ToShortContainerName()}] is being removed.");
                transaction.DeleteContainer(containerId);
            }
        }
    }
}