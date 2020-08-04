using System;
using System.Collections.Concurrent;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Emissary.Models;
using NLog;

namespace Emissary.Core
{
    public interface IServiceLabelParser
    {
        bool CanParseLabel(string key);
        (bool Success, ServiceLabel Result) TryParseValue(string value, params int[] ports);
    }

    public class ServiceLabelParser : IServiceLabelParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<CacheKey, (bool Success, ServiceLabel Result)> _parseCache
            = new ConcurrentDictionary<CacheKey, (bool Success, ServiceLabel Result)>();

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
            var key = new CacheKey(value, ports);
            return _parseCache.GetOrAdd(key, TryParseValue);
        }

        private (bool Success, ServiceLabel Result) TryParseValue(CacheKey cacheKey)
        {
            var parts = cacheKey.Value.Split(';', '.').Select(x => x.Contains("=") ? "--" + x : x);
            var success = false;
            ServiceLabel result = null;
            var parsed = _parser.ParseArguments<ServiceLabel>(parts);

            switch (parsed)
            {
                case Parsed<ServiceLabel> successfulParsed:
                {
                    result = successfulParsed.Value;
                    if (result.ServicePort == null)
                    {
                        switch (cacheKey.DefaultPorts.Length)
                        {
                            case 1:
                                result.ServicePort = cacheKey.DefaultPorts.Single();
                                break;
                            case 0:
                                Logger.Warn($"Failed to parse container option '{cacheKey.Value}' - port was not specifed, but no port was exposed.");
                                break;
                            default:
                                Logger.Warn(
                                    $"Failed to parse container option '{cacheKey.Value}' - port was not specifed, but more than one port was exposed.");
                                break;
                        }
                    }

                    success = result.ServicePort != null;

                    break;
                }

                case NotParsed<ServiceLabel> failedParsed:
                {
                    var builder = SentenceBuilder.Create();
                    var errorText = HelpText.RenderParsingErrorsTextAsLines(failedParsed, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 0);
                    Logger.Warn($"Failed to parse container option '{cacheKey.Value}' - {errorText}.");
                    break;
                }
            }

            return (success, result);
        }

        private class CacheKey
        {
            public string Value { get; }

            public int[] DefaultPorts { get; }

            public CacheKey(string value, int[] defaultPorts)
            {
                Value = value;
                DefaultPorts = defaultPorts;
            }

            private bool Equals(CacheKey other)
            {
                return string.Equals(Value, other.Value) && DefaultPortsEquals(other.DefaultPorts);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals((CacheKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Value.GetHashCode() * 397) ^ GetDefaultPortsHashCode();
                }
            }

            private int GetDefaultPortsHashCode()
            {
                var code = DefaultPorts.Length;
                for (var i = 0; i < DefaultPorts.Length; ++i)
                {
                    code = unchecked(code * 17 + DefaultPorts[i]);
                }

                return code;
            }

            private bool DefaultPortsEquals(int[] target)
            {
                return target.SequenceEqual(DefaultPorts);
            }
        }
    }
}