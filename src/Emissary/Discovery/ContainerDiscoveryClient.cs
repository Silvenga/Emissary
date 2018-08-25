using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;

using Emissary.Core;

namespace Emissary.Discovery
{
    public class ContainerDiscoveryClient
    {
        private readonly DockerClient _client;
        private readonly ServiceLabelParser _labelParser;

        public ContainerDiscoveryClient(DockerClient client, ServiceLabelParser labelParser)
        {
            _client = client;
            _labelParser = labelParser;
        }

        public async Task<IReadOnlyCollection<DiscoveredContainer>> GetContainers(CancellationToken cancellationToken)
        {
            var result = await _client.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = true
            }, cancellationToken);

            var containers = result
                             .Where(x => x.State == "running")
                             .Select(x => new DiscoveredContainer
                             {
                                 Id = x.ID,
                                 Labels = x.Labels,
                                 Created = x.Created,
                                 Names = x.Names,
                                 State = x.State,
                                 Status = x.Status
                             }).ToList();

            return containers;
        }

        public async Task<IReadOnlyList<ContainerService>> GetContainerServices(CancellationToken cancellationToken)
        {
            var containers = await GetContainers(cancellationToken);
            var services = from container in containers
                           from label in container.Labels.Where(x => _labelParser.CanParseLabel(x.Key))
                           let parseResult = _labelParser.TryParseValue(label.Value)
                           let service = parseResult.Result
                           where parseResult.Success
                           select new ContainerService
                           {
                               ContainerId = container.Id,
                               ServicePort = service.ServicePort,
                               ServiceName = service.ServiceName,
                               ServiceTags = service.ServiceTags
                           };
            return services.ToList();
        }
    }

    public class DiscoveredContainer
    {
        public string Id { get; set; }
        public IDictionary<string, string> Labels { get; set; }
        public DateTime Created { get; set; }
        public IList<string> Names { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
    }
}