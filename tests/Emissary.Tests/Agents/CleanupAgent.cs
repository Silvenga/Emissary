using System.Threading;

using Emissary.Agents;
using Emissary.Core;

using NSubstitute;

using Xunit;

namespace Emissary.Tests.Agents
{
    public class CleanupAgentFacts
    {
        private readonly IAgent _agent = new CleanupAgent();
        private readonly IContainerRegistrar _containerRegistrar;

        public CleanupAgentFacts()
        {
            _containerRegistrar = Substitute.For<IContainerRegistrar>();
        }


        [Fact]
        public void When_the_a_cancelation_occurs_then_cleanup_existing_containers()
        {
            var source = new CancellationTokenSource();
            _agent.Monitor(_containerRegistrar, source.Token);

            // Act
            source.Cancel();

            // Assert
        }
    }
}