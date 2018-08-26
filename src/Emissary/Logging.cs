using NLog;
using NLog.Config;
using NLog.Targets;

namespace Emissary
{
    public class Logging
    {
        public static void Configure()
        {
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = "${longdate} ${level:uppercase=true} ${logger:shortName=true}: ${message:WithException=true}"
            };
            config.AddTarget(consoleTarget);
            config.AddRuleForAllLevels(consoleTarget);

            LogManager.Configuration = config;
        }
    }
}