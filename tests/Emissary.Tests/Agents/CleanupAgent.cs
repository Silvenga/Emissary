using System.Collections.Generic;
using System.Threading;

using AutoFixture;

using Emissary.Agents;
using Emissary.Core;

using NSubstitute;

using Xunit;

namespace Emissary.Tests.Agents
{
    public class CleanupAgentFacts
    {
        private static readonly Fixture Autofixture = new Fixture();

        private readonly IAgent _agent = new CleanupAgent();
        private readonly IContainerRegistrar _containerRegistrar;

        public CleanupAgentFacts()
        {
            _containerRegistrar = Substitute.For<IContainerRegistrar>();
        }

        [Fact]
        public void When_the_a_cancelation_occurs_then_cleanup_existing_containers()
        {
            var transaction = Substitute.For<IContainerRegistrarTransaction>();
            _containerRegistrar.BeginTransaction().Returns(transaction);

            var containers = Autofixture.Create<List<string>>();
            transaction.GetContainers().Returns(containers);

            var source = new CancellationTokenSource();
            _agent.Monitor(_containerRegistrar, source.Token);

            // Act
            source.Cancel();

            // Assert
            foreach (var container in containers)
            {
                transaction.Received().DeleteContainer(container);
            }
        }
    }
}