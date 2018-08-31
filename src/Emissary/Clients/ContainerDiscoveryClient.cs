using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;

using Emissary.Core;
using Emissary.Models;

namespace Emissary.Clients
{
    public class ContainerDiscoveryClient
    {
        private readonly IDockerClient _client;
        private readonly IServiceLabelParser _labelParser;

        public ContainerDiscoveryClient(IDockerClient client, IServiceLabelParser labelParser)
        {
            _client = client;
            _labelParser = labelParser;
        }

        public async Task<IReadOnlyCollection<DiscoveredContainer>> GetContainers(CancellationToken cancellationToken)
        {
            var result = await _client.Containers.ListContainersAsync(new ContainersListParameters(), cancellationToken);

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

        public async Task<IReadOnlyList<ContainerService>> GetContainerService(string id, CancellationToken cancellationToken)
        {
            var result = await _client.Containers.InspectContainerAsync(id, cancellationToken);

            var services = from container in new[] { result }.Where(x => x.State.Status == "running")
                           let ports = GetPorts(container).ToArray()
                           from label in GetLabels(container).Where(x => _labelParser.CanParseLabel(x.Key))
                           let parseResult = _labelParser.TryParseValue(label.Value, ports)
                           let service = parseResult.Result
                           where parseResult.Success
                           select new ContainerService
                           {
                               ContainerId = container.ID,
                               // ReSharper disable once PossibleInvalidOperationException
                               ServicePort = service.ServicePort.Value,
                               ServiceName = service.ServiceName,
                               ServiceTags = service.ServiceTags,
                               ContainerStatus = container.State.Status,
                               ContainerCreationOn = container.Created,
                               ContainerState = container.State.Status
                           };
            return services.ToList();
        }

        private IEnumerable<int> GetPorts(ContainerInspectResponse response)
        {
            var result = from port in response?.NetworkSettings?.Ports ?? Enumerable.Empty<KeyValuePair<string, IList<PortBinding>>>()
                         from portBinding in port.Value ?? Enumerable.Empty<PortBinding>()
                         let hostPort = portBinding.HostPort
                         where int.TryParse(hostPort, out _)
                         select int.Parse(hostPort);

            return result;
        }

        private IEnumerable<KeyValuePair<string, string>> GetLabels(ContainerInspectResponse response)
        {
            return response?.Config?.Labels ?? Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public async Task<IReadOnlyList<ContainerService>> GetRunningContainerServices(CancellationToken cancellationToken)
        {
            var dockerResult = await _client.Containers.ListContainersAsync(new ContainersListParameters(), cancellationToken);

            var services = from container in dockerResult
                           let ports = container.Ports.Select<Port, int>(x => x.PublicPort).ToArray()
                           from label in container.Labels.Where(x => _labelParser.CanParseLabel(x.Key))
                           let parseResult = _labelParser.TryParseValue(label.Value, ports)
                           let service = parseResult.Result
                           where parseResult.Success
                           select new ContainerService
                           {
                               ContainerId = container.ID,
                               // ReSharper disable once PossibleInvalidOperationException
                               ServicePort = service.ServicePort.Value,
                               ServiceName = service.ServiceName,
                               ServiceTags = service.ServiceTags,
                               ContainerStatus = container.Status,
                               ContainerCreationOn = container.Created,
                               ContainerState = container.State
                           };
            return services.ToList();
        }

        public async Task GetEvents(YieldingProgress<ContainerEvent> yieldingProgress, CancellationToken cancellationToken)
        {
            var progress = new Progress<JSONMessage>(message =>
            {
                var result = new ContainerEvent
                {
                    Status = message.Status,
                    ContainerId = message.ID
                };
                yieldingProgress.Report(result);
            });

            await _client.System.MonitorEventsAsync(new ContainerEventsParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {
                        "type", new Dictionary<string, bool>
                        {
                            {"container", true}
                        }
                    }
                }
            }, progress, cancellationToken);
        }
    }

    public class YieldingProgress<T>
    {
        private readonly BlockingCollection<T> _blockingCollection = new BlockingCollection<T>();

        public void Report(T value)
        {
            _blockingCollection.Add(value);
        }

        public IEnumerable<T> GetEnumerable(CancellationToken cancellationToken)
        {
            return _blockingCollection.GetConsumingEnumerable(cancellationToken);
        }
    }
}