using System.Threading;
using System.Threading.Tasks;

using Emissary.Core;

namespace Emissary.Agents
{
    public interface IAgent
    {
        Task Monitor(IContainerRegistrar registrar, CancellationToken token);
    }
}