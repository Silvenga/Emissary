using System.Threading;

using Emissary.Core;

using NLog;

namespace Emissary.Agents
{
    public class CleanupAgent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Monitor(ContainerRegistrar registrar, CancellationToken token)
        {
            token.Register(() => Cleanup(registrar));
        }

        private void Cleanup(ContainerRegistrar registrar)
        {
            Logger.Info("A shutdown event has been captured, will begin cleanup logic.");

            registrar.Operate(transaction =>
            {
                foreach (var containerId in transaction.GetContainers())
                {
                    Logger.Info($"Removing container [{containerId}].");
                    transaction.DeleteContainer(containerId);
                }
            });

            Logger.Info("Cleanup completed.");
        }
    }
}