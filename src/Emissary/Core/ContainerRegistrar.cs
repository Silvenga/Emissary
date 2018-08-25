﻿using System;
using System.Collections.Generic;
using System.Linq;

using Emissary.Core.Events;
using Emissary.Models;

namespace Emissary.Core
{
    public class ContainerRegistrar
    {
        private readonly object _rock = new object();

        private readonly Dictionary<string, Dictionary<string, ContainerService>> _containerServices =
            new Dictionary<string, Dictionary<string, ContainerService>>();

        public event EventHandler<ContainerCreatedEventArgs> ContainerCreated;
        public event EventHandler<ContainerDeletedEventArgs> ContainerDeleted;
        public event EventHandler<ContainerServiceCreatedEventArgs> ContainerServiceCreated;

        public void Operate(Action<ContainerRegistrarTransaction> action)
        {
            lock (_rock)
            {
                action(new ContainerRegistrarTransaction(this));
            }
        }

        protected virtual void OnContainerCreated(ContainerCreatedEventArgs e)
        {
            ContainerCreated?.Invoke(this, e);
        }

        protected virtual void OnContainerDeleted(ContainerDeletedEventArgs e)
        {
            ContainerDeleted?.Invoke(this, e);
        }

        protected virtual void OnContainerServiceCreated(ContainerServiceCreatedEventArgs e)
        {
            ContainerServiceCreated?.Invoke(this, e);
        }

        public class ContainerRegistrarTransaction
        {
            private readonly ContainerRegistrar _registrar;

            private readonly Dictionary<string, Dictionary<string, ContainerService>> _containerServices;

            public ContainerRegistrarTransaction(ContainerRegistrar registrar)
            {
                _registrar = registrar;
                _containerServices = registrar._containerServices;
            }

            public void AddContainerService(ContainerService service)
            {
                var containerExists = _containerServices.ContainsKey(service.ContainerId);
                if (!containerExists)
                {
                    _containerServices.Add(service.ContainerId, new Dictionary<string, ContainerService>());
                    _registrar.OnContainerCreated(new ContainerCreatedEventArgs(service.ContainerId));
                }

                var set = _containerServices[service.ContainerId];
                var success = set.TryAdd(service.ServiceName, service);

                if (!success)
                {
                    throw new Exception($"A service with the name {service.ServiceName} for container {service.ContainerId} already exists.");
                }

                _registrar.OnContainerServiceCreated(new ContainerServiceCreatedEventArgs(service.ContainerId, service));
            }

            public void DeleteContainer(string containerId)
            {
                var success = _containerServices.TryGetValue(containerId, out var services);
                if (!success)
                {
                    throw new Exception($"Container {containerId} does not exist.");
                }

                _registrar.OnContainerDeleted(new ContainerDeletedEventArgs(containerId, services.Values.ToList()));

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
        }
    }
}