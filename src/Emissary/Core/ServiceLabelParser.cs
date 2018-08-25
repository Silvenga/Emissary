using System;
using System.Linq;

using CommandLine;

using Emissary.Models;

namespace Emissary.Core
{
    public class ServiceLabelParser
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

        public (bool Success, ServiceLabel Result) TryParseValue(string value)
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

            return (success, result);
        }
    }
}