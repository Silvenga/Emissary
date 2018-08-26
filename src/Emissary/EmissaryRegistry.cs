using System;
using System.IO;

using Consul;

using Docker.DotNet;

using Emissary.Agents;

using Lamar;

using Microsoft.Extensions.Configuration;

namespace Emissary
{
    public class EmissaryRegistry : ServiceRegistry
    {
        public EmissaryRegistry()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                                           .SetBasePath(Directory.GetCurrentDirectory())
                                           .AddJsonFile("appsettings.json", true)
                                           .AddEnvironmentVariables()
                                           .Build();

            For<IConfiguration>().Use(configuration);

            For<DockerClientConfiguration>().Use(x =>
            {
                var config = x.GetInstance<IConfiguration>();
                return new DockerClientConfiguration(new Uri(config["Docker:Host"] ?? "unix:///var/run/docker.sock"));
            });
            For<DockerClient>().Use(x =>
            {
                var config = x.GetInstance<DockerClientConfiguration>();
                return config.CreateClient();
            });

            For<ConsulClient>().Use(x =>
            {
                var config = x.GetInstance<IConfiguration>();
                return new ConsulClient(clientConfiguration =>
                {
                    clientConfiguration.Token = config["Consul:Token"];
                    clientConfiguration.Address = new Uri(config["Consul:Host"] ?? "http://localhost:8500");
                    clientConfiguration.Datacenter = config["Consul:Datacenter"];
                });
            });

            For<IAgent>().Add<ContainerDiscoveryAgent>();
            For<IAgent>().Add<ServiceRegistrationAgent>();
            For<IAgent>().Add<CleanupAgent>();
        }
    }
}