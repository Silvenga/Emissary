using System.Threading;
using System.Threading.Tasks;

using Emissary.Core;

using NLog;

namespace Emissary.Agents
{
    public class CleanupAgent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Monitor(IContainerRegistrar registrar, CancellationToken token)
        {
            token.Register(async () => await Cleanup(registrar));
        }

        private async Task Cleanup(IContainerRegistrar registrar)
        {
            Logger.Info("A shutdown event has been captured, will begin cleanup logic.");

            using (var transaction = await registrar.BeginTransaction())
            {
                foreach (var containerId in transaction.GetContainers())
                {
                    Logger.Info($"Removing container [{containerId.ToShortContainerName()}].");
                    transaction.DeleteContainer(containerId);
                }
            }

            Logger.Info("Cleanup completed.");
        }
    }
}