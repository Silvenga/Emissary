using System;

using Emissary.Models;

namespace Emissary.Core.Events
{
    public class ContainerServiceUpdatedEventArgs : EventArgs
    {
        public string ContainerId { get; set; }

        public ContainerService ContainerService { get; set; }

        public ContainerServiceUpdatedEventArgs(string containerId, ContainerService containerService)
        {
            ContainerId = containerId;
            ContainerService = containerService;
        }
    }
}