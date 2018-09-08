using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Emissary.Core;

using Microsoft.Extensions.Configuration;

namespace Emissary
{
    public class EmissaryConfiguration
    {
        private readonly IConfiguration _configuration;

        public EmissaryConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Required, ValidUri]
        public string DockerHost => _configuration["Docker:Host"] ?? "unix:///var/run/docker.sock";

        public string ConsulToken => _configuration["Consul:Token"];

        [Required, ValidUri(AllowedSchemas = new[] { "http", "https" })]
        public string ConsulHost => _configuration["Consul:Host"] ?? "http://localhost:8500";

        public string ConsulDatacenter => _configuration["Consul:Datacenter"];

        [Required, RegularExpression("\\d+")]
        public string PollingInterval => _configuration["PollingInterval"] ?? TimeSpan.FromMinutes(30).TotalSeconds.ToString(CultureInfo.InvariantCulture);

        public bool Validate(out List<ValidationResult> results)
        {
            var context = new ValidationContext(this, null, null);
            results = new List<ValidationResult>();

            var result = Validator.TryValidateObject(this, context, results, true);
            return result;
        }
    }
}