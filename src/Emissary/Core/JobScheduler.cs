using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NLog;

namespace Emissary.Core
{
    [UsedImplicitly]
    public class JobScheduler
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IList<Task> _knownTasks = new List<Task>();

        public int JobsCount => _knownTasks.Count;

        public async Task ScheduleRecurring<T>(Func<Task> action, CancellationToken token, TimeSpan delayBetweenLoops = default)
        {
            try
            {
                await _semaphore.WaitAsync(token);
                delayBetweenLoops = delayBetweenLoops == default ? TimeSpan.FromSeconds(5) : delayBetweenLoops;
                var logger = LogManager.GetLogger(typeof(T).Name, typeof(T));
                var task = Task.Factory.StartNew(() => PollingLoop(action, delayBetweenLoops, logger, token), TaskCreationOptions.LongRunning);
                _knownTasks.Add(task);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task WaitForAllTasks()
        {
            try
            {
                await _semaphore.WaitAsync();
                await Task.WhenAll(_knownTasks);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task PollingLoop(Func<Task> action, TimeSpan delayBetweenLoops, ILogger jobLogger, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await action();
                }
                catch (Exception e)
                {
                    if (!token.IsCancellationRequested)
                    {
                        jobLogger.Warn(e, "Job failed while executing, the exception was handled.");
                    }
                }

                await Task.Delay(delayBetweenLoops, token);
            }
        }
    }
}