using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Emissary.Agents;

using NLog;

namespace Emissary.Core
{
    public class Mission
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ContainerRegistrar _registrar;
        private readonly ICollection<IAgent> _agents;
        private readonly JobScheduler _scheduler;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public Mission(ContainerRegistrar registrar, List<IAgent> agents, JobScheduler scheduler)
        {
            _registrar = registrar;
            _agents = agents;
            _scheduler = scheduler;
        }

        public Task Start(string[] args)
        {
            Logger.Info("The emissary mission has begun.");

            Logger.Info($"Starting agents [{string.Join(", ", _agents.Select(x => x.GetType().Name))}].");
            foreach (var agent in _agents)
            {
                agent.Monitor(_registrar, _tokenSource.Token);
            }
            Logger.Info("Completed starting agents.");

            Logger.Info("The emissary mission has started.");

            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            Logger.Info("The emissary mission has ended. Beginning shutdown logic.");

            Logger.Info("Notifing all the agents to stop.");
            _tokenSource.Cancel();

            Logger.Info("Waiting for all outstanding jobs to complete.");
            await _scheduler.WaitForAllTasks();

            Logger.Info("Shutdown complete.");
        }
    }
}