using System;

using Emissary.Models;

namespace Emissary.Core.Events
{
    public class ContainerServiceCreatedEventArgs : EventArgs
    {
        public string ContainerId { get; set; }

        public ContainerService ContainerService { get; set; }

        public ContainerServiceCreatedEventArgs(string containerId, ContainerService containerService)
        {
            ContainerId = containerId;
            ContainerService = containerService;
        }
    }
}