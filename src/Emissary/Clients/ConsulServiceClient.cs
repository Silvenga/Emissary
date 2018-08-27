using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Consul;

using Emissary.Models;

namespace Emissary.Clients
{
    public class ConsulServiceClient : IDisposable
    {
        private readonly ConsulClient _client;

        public ConsulServiceClient(ConsulClient client)
        {
            _client = client;
        }

        public async Task<IReadOnlyCollection<ContainerServiceCheck>> GetRegisteredServicesAndChecks(CancellationToken cancellationToken)
        {
            var services = await _client.Agent.Services(cancellationToken);
            var checks = await _client.Agent.Checks(cancellationToken);

            var result = from service in services.Response.Values
                         from check in checks.Response.Values.Where(x => x.ServiceID == service.ID)
                         let meta = service.Meta.Where(x => x.Key.StartsWith("Emissary-"))
                         where meta.Any(x => x.Key == "Emissary-IsManaged")
                         let containerId = meta.Single(x => x.Key == "Emissary-ContainerId").Value
                         select new ContainerServiceCheck
                         {
                             ContainerId = containerId,
                             ServiceId = service.ID,
                             CheckId = check.CheckID
                         };

            return result.ToList();
        }

        public async Task RegisterContainerService(ContainerService containerService, TimeSpan ttl, CancellationToken cancellationToken)
        {
            var service = new AgentServiceRegistration
            {
                ID = containerService.ServiceName + "_" + containerService.ContainerId,
                Meta = new Dictionary<string, string>
                {
                    {"Emissary-ContainerId", containerService.ContainerId},
                    {"Emissary-IsManaged", true.ToString()}
                },
                Tags = containerService.ServiceTags.ToArray(),
                Port = containerService.ServicePort,
                Name = containerService.ServiceName,
                Check = new AgentServiceCheck
                {
                    TTL = ttl,
                    DeregisterCriticalServiceAfter = ttl * 2
                }
            };
            await _client.Agent.ServiceRegister(service, cancellationToken);
        }

        public async Task DeregisterContainerService(string containerId, string serviceName, CancellationToken cancellationToken)
        {
            await _client.Agent.ServiceDeregister(serviceName + "_" + containerId, cancellationToken);
        }

        public async Task MaintainService(string checkId, string status, CancellationToken cancellationToken)
        {
            await _client.Agent.UpdateTTL(checkId, status, TTLStatus.Pass, cancellationToken);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

    public class ContainerServiceCheck
    {
        public string ContainerId { get; set; }

        public string ServiceId { get; set; }

        public string CheckId { get; set; }
    }
}