using System.Threading;
using System.Threading.Tasks;

using Emissary.Discovery;

using NLog;

namespace Emissary.Core
{
    public class Mission
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ContainerRegistrar _registrar;
        private readonly PollingAgent _pollingAgent;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public Mission(ContainerRegistrar registrar, PollingAgent pollingAgent)
        {
            _registrar = registrar;
            _pollingAgent = pollingAgent;
        }

        public async Task Start(string[] args)
        {
            Logger.Info("The emissary mission has begun.");

            Logger.Info("Starting discovery agents.");
            _pollingAgent.Monitor(_registrar, _tokenSource.Token);

            Logger.Info("The emissary mission has started.");

        }

        public async Task Stop()
        {
            Logger.Info("The emissary mission has ended. Will send shutdown signal to pending tasks.");
            _tokenSource.Cancel();
            Logger.Info("Shutdown complete.");
        }
    }
}