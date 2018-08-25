using System;
using System.Collections.Generic;

using Emissary.Models;

namespace Emissary.Core.Events
{
    public class ContainerDeletedEventArgs : EventArgs
    {
        public string ContainerId { get; set; }

        public IReadOnlyList<ContainerService> Services { get; set; }

        public ContainerDeletedEventArgs(string containerId, IReadOnlyList<ContainerService> services)
        {
            ContainerId = containerId;
            Services = services;
        }
    }
}