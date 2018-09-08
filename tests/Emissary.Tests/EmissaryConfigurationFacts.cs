using AutoFixture;

using FluentAssertions;

using Microsoft.Extensions.Configuration;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Xunit;

namespace Emissary.Tests
{
    public class EmissaryConfigurationFacts
    {
        private static readonly Fixture Autofixture = new Fixture();

        private readonly IConfiguration _mockConfiguration;
        private readonly EmissaryConfiguration _emissaryConfiguration;

        public EmissaryConfigurationFacts()
        {
            _mockConfiguration = Substitute.For<IConfiguration>();
            _emissaryConfiguration = new EmissaryConfiguration(_mockConfiguration);
        }

        [Fact]
        public void Default_configuration_should_be_valid()
        {
            _mockConfiguration[Arg.Any<string>()].ReturnsNull();

            // Act
            var result = _emissaryConfiguration.Validate();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void DockerHost_should_require_a_valid_uri()
        {
            var fakeValue = Autofixture.Create<string>();

            _mockConfiguration[Arg.Any<string>()].ReturnsNull();
            _mockConfiguration["Docker:Host"].Returns(fakeValue);

            // Act
            var result = _emissaryConfiguration.Validate();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ConsulHost_should_require_a_valid_uri()
        {
            var fakeValue = Autofixture.Create<string>();

            _mockConfiguration[Arg.Any<string>()].ReturnsNull();
            _mockConfiguration["Consul:Host"].Returns(fakeValue);

            // Act
            var result = _emissaryConfiguration.Validate();

            // Assert
            result.Should().BeFalse();
        }
    }
}