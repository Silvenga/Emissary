using System;
using System.Threading;
using System.Threading.Tasks;

using Emissary.Core;

using Lamar;

namespace Emissary
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            Logging.Configure();

            var container = new Container(registry => registry.IncludeRegistry<EmissaryRegistry>());

            var mission = container.GetInstance<Mission>();

            await mission.Start(args);
            await WaitForExit();
            await mission.Stop();
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