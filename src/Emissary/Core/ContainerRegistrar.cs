using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Emissary.Core.Events;
using Emissary.Models;

using JetBrains.Annotations;

namespace Emissary.Core
{
    public interface IContainerRegistrar
    {
        event EventHandler<ContainerCreatedEventArgs> ContainerCreated;
        event EventHandler<ContainerDeletedEventArgs> ContainerDeleted;
        event EventHandler<ContainerServiceCreatedEventArgs> ContainerServiceCreated;
        event EventHandler<ContainerServiceUpdatedEventArgs> ContainerServiceUpdated;
        Task<IContainerRegistrarTransaction> BeginTransaction();
    }

    public interface IContainerRegistrarTransaction : IDisposable
    {
        void AddContainerService(ContainerService service);
        void UpdateContainerService(ContainerService service);
        void DeleteContainer(string containerId);
        IReadOnlyList<string> GetContainers();
        IReadOnlyList<ContainerService> GetAllContainerServices();
        IReadOnlyList<ContainerService> GetContainerServices(string containerId);
        ContainerService GetContainerService(string containerId, string serviceName);
        bool ContainerServiceExists(string containerId, string serviceName);
    }

    [UsedImplicitly]
    public class ContainerRegistrar : IContainerRegistrar
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly Dictionary<string, Dictionary<string, ContainerService>> _containerServices =
            new Dictionary<string, Dictionary<string, ContainerService>>();

        public event EventHandler<ContainerCreatedEventArgs> ContainerCreated;
        public event EventHandler<ContainerDeletedEventArgs> ContainerDeleted;
        public event EventHandler<ContainerServiceCreatedEventArgs> ContainerServiceCreated;
        public event EventHandler<ContainerServiceUpdatedEventArgs> ContainerServiceUpdated;

        public async Task<IContainerRegistrarTransaction> BeginTransaction()
        {
            await _semaphore.WaitAsync();
            return new ContainerRegistrarTransaction(this, () => _semaphore.Release());
        }

        private void OnContainerCreated(ContainerRegistrarTransaction transaction, ContainerCreatedEventArgs e)
        {
            ContainerCreated?.Invoke(transaction, e);
        }

        private void OnContainerDeleted(ContainerRegistrarTransaction transaction, ContainerDeletedEventArgs e)
        {
            ContainerDeleted?.Invoke(transaction, e);
        }

        private void OnContainerServiceCreated(ContainerRegistrarTransaction transaction, ContainerServiceCreatedEventArgs e)
        {
            ContainerServiceCreated?.Invoke(transaction, e);
        }

        private void OnContainerServiceUpdated(ContainerRegistrarTransaction transaction, ContainerServiceUpdatedEventArgs e)
        {
            ContainerServiceUpdated?.Invoke(transaction, e);
        }

        public class ContainerRegistrarTransaction : IContainerRegistrarTransaction
        {
            private readonly ContainerRegistrar _registrar;
            private readonly Action _disposeAction;

            private readonly Dictionary<string, Dictionary<string, ContainerService>> _containerServices;

            public ContainerRegistrarTransaction(ContainerRegistrar registrar, Action disposeAction)
            {
                _registrar = registrar;
                _disposeAction = disposeAction;
                _containerServices = registrar._containerServices;
            }

            public void AddContainerService(ContainerService service)
            {
                var containerExists = _containerServices.ContainsKey(service.ContainerId);
                if (!containerExists)
                {
                    _containerServices.Add(service.ContainerId, new Dictionary<string, ContainerService>());
                    _registrar.OnContainerCreated(this, new ContainerCreatedEventArgs(service.ContainerId));
                }

                var set = _containerServices[service.ContainerId];
                var success = set.TryAdd(service.ServiceName, service);

                if (!success)
                {
                    throw new Exception($"A service with the name {service.ServiceName} for container {service.ContainerId} already exists.");
                }

                _registrar.OnContainerServiceCreated(this, new ContainerServiceCreatedEventArgs(service.ContainerId, service));
            }

            public void UpdateContainerService(ContainerService service)
            {
                var containerExists = _containerServices.ContainsKey(service.ContainerId)
                                      && _containerServices[service.ContainerId].ContainsKey(service.ServiceName);
                if (!containerExists)
                {
                    throw new Exception($"The service {service.ServiceName} in container {service.ContainerId} does not exist or the container is missing.");
                }

                _containerServices[service.ContainerId][service.ServiceName] = service;

                _registrar.OnContainerServiceUpdated(this, new ContainerServiceUpdatedEventArgs(service.ContainerId, service));
            }

            public void DeleteContainer(string containerId)
            {
                var success = _containerServices.TryGetValue(containerId, out var services);
                if (!success)
                {
                    throw new Exception($"Container {containerId} does not exist.");
                }

                _registrar.OnContainerDeleted(this, new ContainerDeletedEventArgs(containerId, services.Values.ToList()));

                _containerServices.Remove(containerId);
            }

            public IReadOnlyList<string> GetContainers()
            {
                return _containerServices.Keys.Select(x => x).ToList();
            }

            public IReadOnlyList<ContainerService> GetAllContainerServices()
            {
                return _containerServices.Values.SelectMany(x => x.Values).Select(x => x).ToList();
            }

            public IReadOnlyList<ContainerService> GetContainerServices(string containerId)
            {
                return _containerServices[containerId].Values.Select(x => x).ToList();
            }

            public ContainerService GetContainerService(string containerId, string serviceName)
            {
                return _containerServices[containerId][serviceName];
            }

            public bool ContainerServiceExists(string containerId, string serviceName)
            {
                return _containerServices.ContainsKey(containerId) && _containerServices[containerId].ContainsKey(serviceName);
            }

            public void Dispose()
            {
                _disposeAction.Invoke();
            }
        }
    }
}