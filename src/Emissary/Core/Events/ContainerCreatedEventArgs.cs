using System;

namespace Emissary.Core.Events
{
    public class ContainerCreatedEventArgs : EventArgs
    {
        public string ContainerId { get; set; }

        public ContainerCreatedEventArgs(string containerId)
        {
            ContainerId = containerId;
        }
    }
}