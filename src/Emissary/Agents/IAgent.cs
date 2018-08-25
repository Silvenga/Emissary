using System.Threading;

using Emissary.Core;

namespace Emissary.Agents
{
    public interface IAgent
    {
        void Monitor(ContainerRegistrar registrar, CancellationToken token);
    }
}