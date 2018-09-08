using System;

using AutoFixture;

using Emissary.Core;

using FluentAssertions;

using Xunit;

namespace Emissary.Tests.Core
{
    public class ValidUriAttributeFacts
    {
        private static readonly Fixture Autofixture = new Fixture();

        [Fact]
        public void When_validating_correct_uri_then_is_valid_should_return_true()
        {
            var uriFake = Autofixture.Create<Uri>();

            var attribute = new ValidUriAttribute
            {
                AllowedSchemas = new[]
                {
                    uriFake.Scheme
                }
            };

            // Act
            var result = attribute.IsValid(uriFake.ToString());

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void When_validating_invalid_schema_then_is_valid_should_return_false()
        {
            var uriFake = Autofixture.Create<Uri>();
            var schemaFake = Autofixture.Create<string>();

            var attribute = new ValidUriAttribute
            {
                AllowedSchemas = new[]
                {
                    schemaFake
                }
            };

            // Act
            var result = attribute.IsValid(uriFake.ToString());

            // Assert
            result.Should().BeFalse();
        }
    }
}