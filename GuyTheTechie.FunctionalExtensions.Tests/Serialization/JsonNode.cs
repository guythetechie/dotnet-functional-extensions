using FluentAssertions;
using FluentAssertions.LanguageExt;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class JsonNodeTests
{
    [FsCheck.NUnit.Property()]
    public Property TryAsJsonObject_returns_none_if_the_node_is_not_a_json_object()
    {
        var generator = Generator.JsonNode.Where(node => node is JsonObject is false);
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsJsonObject();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsJsonObject_returns_some_if_the_node_is_a_json_object()
    {
        var generator = from jsonObject in Generator.JsonObject
                        let jsonNode = jsonObject as JsonNode
                        select (jsonNode, jsonObject);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, expectedJsonObject) = x;

            // Act
            var option = jsonNode.TryAsJsonObject();

            // Assert
            option.Should().BeSome(value => JsonNode.DeepEquals(value, expectedJsonObject).Should().BeTrue());
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsJsonArray_returns_none_if_the_node_is_not_a_json_array()
    {
        var generator = Generator.JsonNode.Where(node => node is JsonArray is false);
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsJsonArray();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsJsonArray_returns_some_if_the_node_is_a_json_array()
    {
        var generator = from jsonArray in Generator.JsonArray
                        let jsonNode = jsonArray as JsonNode
                        select (jsonNode, jsonArray);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, expectedJsonArray) = x;

            // Act
            var option = jsonNode.TryAsJsonArray();

            // Assert
            option.Should().BeSome(value => JsonNode.DeepEquals(value, expectedJsonArray).Should().BeTrue());
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsJsonValue_returns_none_if_the_node_is_not_a_json_value()
    {
        var generator = Generator.JsonNode.Where(node => node is JsonValue is false);
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsJsonValue();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsJsonValue_returns_some_if_the_node_is_a_json_value()
    {
        var generator = from jsonValue in Generator.JsonValue
                        let jsonNode = jsonValue as JsonNode
                        select (jsonNode, jsonValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, expectedJsonValue) = x;

            // Act
            var option = jsonNode.TryAsJsonValue();

            // Assert
            option.Should().BeSome(value => JsonNode.DeepEquals(value, expectedJsonValue).Should().BeTrue());
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsString_returns_none_if_the_node_is_not_a_string()
    {
        var generator = Generator.JsonNode.Where(node => node is JsonValue value
                                                         && value.TryGetValue<string>(out var _) is false);
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsString();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsString_returns_some_if_the_node_is_a_string()
    {
        var generator = from stringValue in Generator.GenerateDefault<string>()
                        let jsonNode = JsonValue.Create(stringValue) as JsonNode
                        select (jsonNode, stringValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, expectedString) = x;

            // Act
            var option = jsonNode.TryAsString();

            // Assert
            option.Should().BeSome(value => JsonNode.DeepEquals(value, expectedString).Should().BeTrue());
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsGuid_returns_none_if_the_node_is_not_a_guid()
    {
        var generator = Generator.JsonNode
                                 .Where(node => node is not JsonValue value
                                                || (value.TryGetValue<Guid>(out var _) is false
                                                    && value.TryGetValue<string>(out var stringValue) is false
                                                    && Guid.TryParse(stringValue, out var _) is false));

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsGuid();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsGuid_returns_some_if_the_node_is_a_guid()
    {
        var generator = from guidValue in Generator.GenerateDefault<Guid>()
                        from jsonValue in Gen.Elements(JsonValue.Create(guidValue), JsonValue.Create(guidValue.ToString()))
                        select (jsonValue as JsonNode, guidValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, guidValue) = x;

            // Act
            var option = jsonNode.TryAsGuid();

            // Assert
            option.Should().BeSome().Which.Should().Be(guidValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsAbsoluteUri_returns_none_if_the_node_is_not_an_absolute_uri()
    {
        var generator = Generator.JsonNode
                                 .Where(node => node is not JsonValue value
                                                || (value.TryGetValue<string>(out var stringValue) is false
                                                    && Uri.TryCreate(stringValue, UriKind.Absolute, out var _) is false));

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsAbsoluteUri();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsAbsoluteUri_returns_some_if_the_node_is_an_absolute_uri()
    {
        var generator = from uri in Generator.AbsoluteUri
                        let jsonValue = JsonValue.Create(uri.ToString())
                        select (jsonValue as JsonNode, uri);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, uri) = x;

            // Act
            var option = jsonNode.TryAsAbsoluteUri();

            // Assert
            option.Should().BeSome().Which.Should().Be(uri);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDateTimeOffset_returns_none_if_the_node_is_not_a_DateTimeOffset()
    {
        var generator = Generator.JsonNode
                                 .Where(node => node is not JsonValue value
                                                || (value.TryGetValue<DateTimeOffset>(out var _) is false
                                                    && value.TryGetValue<string>(out var stringValue) is false
                                                    && DateTimeOffset.TryParse(stringValue, out var _) is false));

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsDateTimeOffset();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDateTimeOffset_returns_some_if_the_node_is_a_DateTimeOffset()
    {
        var generator = from dateTimeOffsetValue in Generator.GenerateDefault<DateTimeOffset>()
                        from jsonValue in Gen.Elements(JsonValue.Create(dateTimeOffsetValue), JsonValue.Create(dateTimeOffsetValue.ToString("O")))
                        select (jsonValue as JsonNode, dateTimeOffsetValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, dateTimeOffsetValue) = x;

            // Act
            var option = jsonNode.TryAsDateTimeOffset();

            // Assert
            option.Should().BeSome().Which.Should().BeCloseTo(dateTimeOffsetValue, TimeSpan.FromSeconds(1));
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDateTime_returns_none_if_the_node_is_not_a_DateTime()
    {
        var generator = Generator.JsonNode
                                 .Where(node => node is not JsonValue value
                                                || (value.TryGetValue<DateTime>(out var _) is false
                                                    && value.TryGetValue<string>(out var stringValue) is false
                                                    && DateTime.TryParse(stringValue, out var _) is false));

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsDateTime();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDateTime_returns_some_if_the_node_is_a_DateTime()
    {
        var generator = from dateTimeValue in Generator.GenerateDefault<DateTime>()
                        from jsonValue in Gen.Elements(JsonValue.Create(dateTimeValue), JsonValue.Create(dateTimeValue.ToString("s")))
                        select (jsonValue as JsonNode, dateTimeValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, dateTimeValue) = x;

            // Act
            var option = jsonNode.TryAsDateTime();

            // Assert
            option.Should().BeSome().Which.Should().BeCloseTo(dateTimeValue, TimeSpan.FromSeconds(1));
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsInt_returns_none_if_the_node_is_not_an_int()
    {
        var generator = Generator.JsonNode.Where(node => node is JsonValue value
                                                         && value.TryGetValue<int>(out var _) is false);
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsInt();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsInt_returns_some_if_the_node_is_an_int()
    {
        var generator = from intValue in Generator.GenerateDefault<int>()
                        let jsonNode = JsonValue.Create(intValue) as JsonNode
                        select (jsonNode, intValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, expectedInt) = x;

            // Act
            var option = jsonNode.TryAsInt();

            // Assert
            option.Should().BeSome().Which.Should().Be(expectedInt);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDouble_returns_none_if_the_node_is_not_a_double()
    {
        var generator = Generator.JsonNode.Where(node => node is JsonValue value
                                                         && value.TryGetValue<double>(out var _) is false);
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsDouble();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDouble_returns_some_if_the_node_is_a_double()
    {
        var generator = from doubleValue in Generator.GenerateDefault<double>()
                                                     .Where(double.IsFinite)
                                                     .Where(d => double.IsNaN(d) is false)
                        let jsonNode = JsonValue.Create(doubleValue) as JsonNode
                        select (jsonNode, doubleValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, expectedDouble) = x;

            // Act
            var option = jsonNode.TryAsDouble();

            // Assert
            option.Should().BeSome().Which.Should().Be(expectedDouble);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsBool_returns_none_if_the_node_is_not_a_bool()
    {
        var generator = Generator.JsonNode.Where(node => node is JsonValue value
                                                         && value.TryGetValue<bool>(out var _) is false);
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonNode =>
        {
            // Act
            var option = jsonNode.TryAsBool();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsBool_returns_some_if_the_node_is_a_bool()
    {
        var generator = from boolValue in Generator.GenerateDefault<bool>()
                        let jsonNode = JsonValue.Create(boolValue) as JsonNode
                        select (jsonNode, boolValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonNode, expectedBool) = x;

            // Act
            var option = jsonNode.TryAsBool();

            // Assert
            option.Should().BeSome();
            option.ValueUnsafe().Should().Be(expectedBool);
        });
    }
}