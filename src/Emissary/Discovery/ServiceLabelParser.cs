using System;
using System.Collections.Generic;
using System.Linq;

using CommandLine;

namespace Emissary.Discovery
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

    public class ServiceLabel
    {
        [Value(0, Required = true)]
        public string ServiceName { get; set; }

        [Value(1, Required = true)]
        public int ServicePort { get; set; }

        [Option("tags", Separator = ',')]
        public IReadOnlyList<string> ServiceTags { get; set; }
    }
}