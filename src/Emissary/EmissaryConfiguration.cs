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

        public string DockerHost => _configuration["Docker:Host"] ?? "unix:///var/run/docker.sock";
        public string ConsulToken => _configuration["Consul:Token"];
        public string ConsulHost => _configuration["Consul:Host"] ?? "http://localhost:8500";
        public string ConsulDatacenter => _configuration["Consul:Datacenter"];
    }
}