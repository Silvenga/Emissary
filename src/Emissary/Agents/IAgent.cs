using System.Threading;

using Emissary.Core;

namespace Emissary.Agents
{
    public interface IAgent
    {
        void Monitor(IContainerRegistrar registrar, CancellationToken token);
    }
}