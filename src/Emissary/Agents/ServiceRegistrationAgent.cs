using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Emissary.Clients;
using Emissary.Core;
using Emissary.Core.Events;
using Emissary.Models;

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

        public void Monitor(ContainerRegistrar registrar, CancellationToken token)
        {
            registrar.ContainerServiceCreated += (sender, args) => RegistrarOnContainerServiceCreated(args);
            registrar.ContainerDeleted += (sender, args) => RegistrarOnContainerDeleted(args);

            _scheduler.ScheduleRecurring<ServiceRegistrationAgent>(() => MaintenanceLoop(registrar, token), token);
        }

        private void RegistrarOnContainerServiceCreated(ContainerServiceCreatedEventArgs e)
        {
            _client.RegisterContainerService(e.ContainerService, TimeSpan.FromSeconds(30), CancellationToken.None).Wait();

            Logger.Info($"Registering service [{e.ContainerService.ServiceName}] for container [{e.ContainerId}].");
        }

        private void RegistrarOnContainerDeleted(ContainerDeletedEventArgs e)
        {
            foreach (var service in e.Services)
            {
                _client.DeregisterContainerService(e.ContainerId, service.ServiceName, CancellationToken.None).Wait();
            }

            Logger.Info($"Deregistered container [{e.ContainerId}] services [{string.Join(", ", e.Services.Select(x => x.ServiceName))}].");
        }

        private async Task MaintenanceLoop(ContainerRegistrar registrar, CancellationToken token)
        {
            var consulChecks = await _client.GetRegisteredServicesAndChecks(token);
            IReadOnlyList<ContainerService> desiredContainerServices = null;

            registrar.Operate(transation => { desiredContainerServices = transation.GetAllContainerServices(); });

            var checks = from desiredService in desiredContainerServices
                         from consulService in consulChecks.Where(x => x.ContainerId == desiredService.ContainerId)
                         select new
                         {
                             desiredService.ContainerStatus,
                             consulService.CheckId
                         };

            foreach (var check in checks)
            {
                await _client.MaintainService(check.CheckId, check.ContainerStatus, token);
            }
        }
    }
}