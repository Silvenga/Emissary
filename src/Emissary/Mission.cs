using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Emissary.Agents;
using Emissary.Core;

using JetBrains.Annotations;

using NLog;

namespace Emissary
{
    [UsedImplicitly]
    public class Mission
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IContainerRegistrar _registrar;
        private readonly Lazy<List<IAgent>> _lazyAgents;
        private readonly JobScheduler _scheduler;
        private readonly EmissaryConfiguration _configuration;

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public Mission(IContainerRegistrar registrar, Lazy<List<IAgent>> lazyAgents, JobScheduler scheduler, EmissaryConfiguration configuration)
        {
            _registrar = registrar;
            _lazyAgents = lazyAgents;
            _scheduler = scheduler;
            _configuration = configuration;
        }

        public Task<bool> Start(string[] args)
        {
            Logger.Info($"The emissary mission has begun using version {Assembly.GetEntryAssembly().GetName().Version}.");

            var configurationValid = _configuration.Validate(out var results);
            if (!configurationValid)
            {
                foreach (var result in results)
                {
                    Logger.Error(result);
                }

                Logger.Info("Configruations are invalid, will shutdown.");
                return Task.FromResult(false);
            }

            Logger.Info("Configurations are valid.");

            var agents = _lazyAgents.Value;
            foreach (var agent in agents)
            {
                Logger.Info($"Starting agent [{agent}].");
                agent.Monitor(_registrar, _tokenSource.Token);
            }

            Logger.Info($"{_scheduler.JobsCount} jobs are now active.");

            return Task.FromResult(true);
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