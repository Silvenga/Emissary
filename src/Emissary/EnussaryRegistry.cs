using System;
using System.IO;

using CommandLine;

using Docker.DotNet;

using Emissary.Core;

using Lamar;

using Microsoft.Extensions.Configuration;

namespace Emissary
{
    public class EnussaryRegistry : ServiceRegistry
    {
        public EnussaryRegistry()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                                           .SetBasePath(Directory.GetCurrentDirectory())
                                           .AddJsonFile("appsettings.json")
                                           .Build();

            For<IConfiguration>().Use(configuration);
            For<ContainerRegistrar>().Use<ContainerRegistrar>().Singleton();

            For<DockerClientConfiguration>().Use(x =>
            {
                var config = x.GetInstance<IConfiguration>();
                return new DockerClientConfiguration(new Uri(config["Docker:Host"]));
            });

            For<DockerClient>().Use(x =>
            {
                var config = x.GetInstance<DockerClientConfiguration>();
                return config.CreateClient();
            });
        }
    }
}