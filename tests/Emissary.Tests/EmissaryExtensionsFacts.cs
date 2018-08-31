using AutoFixture;

using FluentAssertions;

using Xunit;

namespace Emissary.Tests
{
    public class EmissaryExtensionsFacts
    {
        private static readonly Fixture Autofixture = new Fixture();

        [Fact]
        public void When_given_a_container_id_return_short_id()
        {
            var containerId = Autofixture.Create<string>();

            // Act
            var result = containerId.ToShortContainerName();

            // Assert
            result.Should().HaveLength(12);
        }

        [Fact]
        public void When_given_null_return_null_short_id()
        {
            // Act
            var result = ((string)null).ToShortContainerName();

            // Assert
            result.Should().BeNull();
        }
    }
}