using System;
using System.Linq;

using CommandLine;

using Emissary.Models;

namespace Emissary.Core
{
    public interface IServiceLabelParser
    {
        bool CanParseLabel(string key);
        (bool Success, ServiceLabel Result) TryParseValue(string value, params int[] ports);
    }

    public class ServiceLabelParser : IServiceLabelParser
    {
        private readonly Parser _parser;

        public ServiceLabelParser(Parser parser)
        {
            _parser = parser;
        }

        public bool CanParseLabel(string key)
        {
            return key.StartsWith("com.silvenga.emissary.service", StringComparison.InvariantCultureIgnoreCase);
        }

        public (bool Success, ServiceLabel Result) TryParseValue(string value, params int[] ports)
        {
            var parts = value.Split(";").Select(x => x.Contains("=") ? "--" + x : x);
            var success = false;
            ServiceLabel result = null;
            _parser.ParseArguments<ServiceLabel>(parts)
                   .WithParsed(opts =>
                   {
                       success = true;
                       result = opts;
                   });

            if (success && result.ServicePort == null)
            {
                if (ports.Length == 1)
                {
                    result.ServicePort = ports.Single();
                }
                else
                {
                    success = false;
                }
            }

            return (success, result);
        }
    }
}