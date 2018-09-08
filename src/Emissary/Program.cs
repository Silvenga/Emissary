using System;
using System.Threading;
using System.Threading.Tasks;

using Lamar;

namespace Emissary
{
    public static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            Logging.Configure();

            var container = new Container(registry => registry.IncludeRegistry<EmissaryRegistry>());

            var mission = container.GetInstance<Mission>();

            var success = await mission.Start(args);
            if (success)
            {
                await WaitForExit();
            }
            await mission.Stop();

            return success ? 0 : 1;
        }

        private static async Task WaitForExit()
        {
            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                exitEvent.Set();
                Thread.Sleep(2000);
            };

            await Task.Run(() => exitEvent.WaitOne());
        }
    }
}