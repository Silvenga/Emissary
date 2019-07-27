using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emissary.Clients;
using Emissary.Core;
using Emissary.Core.Events;
using NLog;

namespace Emissary.Agents
{
    public class ServiceRegistrationAgent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly JobScheduler _scheduler;
        private readonly ConsulServiceClient _client;

        public ServiceRegistrationAgent(JobScheduler scheduler, ConsulServiceClient client)
        {
            _scheduler = scheduler;
            _client = client;
        }

        public async Task Monitor(IContainerRegistrar registrar, CancellationToken token)
        {
            registrar.ContainerServiceCreated += RegistrarOnContainerServiceCreated;
            registrar.ContainerDeleted += RegistrarOnContainerDeleted;

            await _scheduler.ScheduleRecurring<ServiceRegistrationAgent>(() => MaintenanceLoop(registrar, token), token);
        }

        private void RegistrarOnContainerServiceCreated(object sender, ContainerServiceCreatedEventArgs e)
        {
            _client.RegisterContainerService(e.ContainerService, TimeSpan.FromSeconds(30), CancellationToken.None).Wait();

            Logger.Info($"Registering service [{e.ContainerService.ServiceName}] for container [{e.ContainerId.ToShortContainerName()}].");
        }

        private void RegistrarOnContainerDeleted(object sender, ContainerDeletedEventArgs e)
        {
            foreach (var service in e.Services)
            {
                try
                {
                    _client.DeregisterContainerService(e.ContainerId, service.ServiceName, CancellationToken.None).Wait();
                }
                catch (Exception exception)
                {
                    Logger.Warn(exception,
                        $"Error when trying to deregistered container [{e.ContainerId.ToShortContainerName()}] service [{service.ServiceName}].");
                }
            }

            Logger.Info(
                $"Deregistered container [{e.ContainerId.ToShortContainerName()}] services [{string.Join(", ", e.Services.Select(x => x.ServiceName))}].");
        }

        private async Task MaintenanceLoop(IContainerRegistrar registrar, CancellationToken token)
        {
            using (var transaction = await registrar.BeginTransaction())
            {
                var desiredServices = transaction.GetAllContainerServices();
                var consulServices = await _client.GetRegisteredServicesAndChecks(token);

                var checks = (from desiredService in desiredServices
                              from consulService in consulServices.Where(x => x.ContainerId == desiredService.ContainerId).DefaultIfEmpty()
                              let status = ""
                                           + $"Container: {desiredService.ContainerId.ToShortContainerName()}\n"
                                           + $"    Image: {desiredService.Image}\n"
                                           + $" Creation: {desiredService.ContainerCreationOn}\n"
                                           + $"    State: {desiredService.ContainerState}\n"
                                           + $"   Status: {desiredService.ContainerStatus}"
                              select new
                              {
                                  desiredService.ContainerId,
                                  Status = status,
                                  consulService?.CheckId,
                                  Missing = consulService == null
                              }).ToList();

                foreach (var check in checks.Where(x => x.Missing))
                {
                    Logger.Warn($"Container [{check.ContainerId.ToShortContainerName()}] disappeared from consul, assuming the container was removed.");
                    transaction.DeleteContainer(check.ContainerId);
                }

                foreach (var check in checks.Where(x => !x.Missing))
                {
                    await _client.MaintainService(check.CheckId, check.Status, token);
                }
            }
        }
    }
}