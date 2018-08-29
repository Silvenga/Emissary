using CommandLine;

using Emissary.Core;

using FluentAssertions;

using Xunit;

namespace Emissary.Tests.Core
{
    public class LabelParser
    {
        private readonly ServiceLabelParser _parser = new ServiceLabelParser(new Parser());

        [Fact]
        public void Can_parse_service_name()
        {
            const string label = "name;80";

            // Act
            var (success, result) = _parser.TryParseValue(label);

            // Assert
            success.Should().BeTrue();
            result.ServiceName.Should().Be("name");
        }

        [Fact]
        public void Can_parse_service_name_with_spaces()
        {
            const string label = "one name;80";

            // Act
            var (success, result) = _parser.TryParseValue(label);

            // Assert
            success.Should().BeTrue();
            result.ServiceName.Should().Be("one name");
        }

        [Fact]
        public void Can_parse_service_port()
        {
            const string label = "name;80";

            // Act
            var (success, result) = _parser.TryParseValue(label);

            // Assert
            success.Should().BeTrue();
            result.ServicePort.Should().Be(80);
        }

        [Fact]
        public void Can_parse_service_tags()
        {
            const string label = "name;80;tags=a,b,c";

            // Act
            var (success, result) = _parser.TryParseValue(label);

            // Assert
            success.Should().BeTrue();
            result.ServiceTags.Should().ContainInOrder("a", "b", "c");
        }

        [Fact]
        public void When_port_is_not_given_can_parse_service_tags()
        {
            const string label = "name;tags=a,b,c";

            // Act
            var (success, result) = _parser.TryParseValue(label, 80);

            // Assert
            success.Should().BeTrue();
            result.ServicePort.Should().Be(80);
            result.ServiceTags.Should().ContainInOrder("a", "b", "c");
        }

        [Fact]
        public void When_label_is_incorrect_do_not_throw()
        {
            const string label = "some value";

            // Act
            var (success, _) = _parser.TryParseValue(label);

            // Assert
            success.Should().BeFalse();
        }

        [Fact]
        public void When_no_port_is_given_use_a_single_default_port()
        {
            const string label = "some value";

            // Act
            var (success, result) = _parser.TryParseValue(label, 80);

            // Assert
            success.Should().BeTrue();
            result.ServicePort.Should().Be(80);
        }

        [Fact]
        public void When_no_port_is_given_and_multiple_ports_exist_fail_parsing()
        {
            const string label = "some value";

            // Act
            var (success, _) = _parser.TryParseValue(label, 80, 81);

            // Assert
            success.Should().BeFalse();
        }

        [Fact]
        public void When_given_correct_label_key_return_true()
        {
            const string key = "com.silvenga.emissary.service";

            // Act
            var result = _parser.CanParseLabel(key);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void When_given_multiple_labels_return_true()
        {
            const string key = "com.silvenga.emissary.service-1";

            // Act
            var result = _parser.CanParseLabel(key);

            // Assert
            result.Should().BeTrue();
        }
    }
}