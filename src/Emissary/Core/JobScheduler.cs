using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NLog;

namespace Emissary.Core
{
    public class JobScheduler
    {
        private readonly ISet<Task> _knownTasks = new HashSet<Task>();

        public void ScheduleRecurring<T>(Func<Task> action, CancellationToken token, TimeSpan delayBetweenLoops = default)
        {
            delayBetweenLoops = delayBetweenLoops == default ? TimeSpan.FromSeconds(5) : delayBetweenLoops;
            var logger = LogManager.GetLogger(typeof(T).Name, typeof(T));
            var task = Task.Factory.StartNew(() => PollingLoop(action, delayBetweenLoops, logger, token), TaskCreationOptions.LongRunning);
            _knownTasks.Add(task);
        }

        public async Task WaitForAllTasks()
        {
            await Task.WhenAll(_knownTasks);
        }

        private async Task PollingLoop(Func<Task> action, TimeSpan delayBetweenLoops, ILogger logger, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await action();
                }
                catch (Exception e)
                {
                    logger.Warn(e, "Job failed while executing, the exception was handled.");
                }

                await Task.Delay(delayBetweenLoops, token);
            }
        }
    }
}