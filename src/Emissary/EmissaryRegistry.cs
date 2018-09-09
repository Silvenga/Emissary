using System;
using System.IO;

using Consul;

using Docker.DotNet;

using Emissary.Agents;
using Emissary.Core;

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
            var emissaryConfiguration = new EmissaryConfiguration(configuration);

            For<EmissaryConfiguration>().Use(emissaryConfiguration);

            For<DockerClientConfiguration>().Use(x =>
            {
                var config = x.GetInstance<EmissaryConfiguration>();
                return new DockerClientConfiguration(new Uri(config.DockerHost));
            });
            For<IDockerClient>().Use(x =>
            {
                var config = x.GetInstance<DockerClientConfiguration>();
                return config.CreateClient();
            });

            For<ConsulClient>().Use(x =>
            {
                var config = x.GetInstance<EmissaryConfiguration>();
                return new ConsulClient(clientConfiguration =>
                {
                    clientConfiguration.Token = config.ConsulToken;
                    clientConfiguration.Address = new Uri(config.ConsulHost);
                    clientConfiguration.Datacenter = config.ConsulDatacenter;
                });
            });

            For<IContainerRegistrar>().Use<ContainerRegistrar>();
            For<JobScheduler>().Use<JobScheduler>().Singleton();
            For<IServiceLabelParser>().Use<ServiceLabelParser>();

            For<IAgent>().Add<PollingContainerDiscoveryAgent>();
            For<IAgent>().Add<SubscribingContainerDiscoveryAgent>();
            For<IAgent>().Add<ServiceRegistrationAgent>();
            For<IAgent>().Add<CleanupAgent>();
        }
    }
}