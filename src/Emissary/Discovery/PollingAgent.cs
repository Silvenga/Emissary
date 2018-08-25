using System;
using System.Threading;
using System.Threading.Tasks;

using Emissary.Core;

using NLog;

namespace Emissary.Discovery
{
    public class PollingAgent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ContainerDiscoveryClient _client;

        public PollingAgent(ContainerDiscoveryClient client)
        {
            _client = client;
        }

        public void Monitor(ContainerRegistrar registrar, CancellationToken token)
        {
            Task.Factory.StartNew(() => PollingLoop(registrar, token), TaskCreationOptions.LongRunning);
        }

        private async Task PollingLoop(ContainerRegistrar registrar, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Poll(registrar, token);
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Handled error while polling. Will wait before attempting again.");
                    await Task.Delay(TimeSpan.FromSeconds(30), token);
                }
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }

        private async Task Poll(ContainerRegistrar registrar, CancellationToken token)
        {
            var containers = await _client.GetContainerServices(token);
            registrar.DisoverContainers(containers);
        }
    }

    public interface IAgent
    {
        void Monitor(ContainerRegistrar registrar, CancellationToken token);
    }
}