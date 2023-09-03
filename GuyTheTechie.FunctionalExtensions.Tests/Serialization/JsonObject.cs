using FluentAssertions;
using FluentAssertions.LanguageExt;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class JsonObjectTests
{
    [FsCheck.NUnit.Property()]
    public Property GetProperty_throws_if_the_property_does_not_exist()
    {
        var generator = from json in Generator.JsonObject
                        from nonExistingKey in GenerateNonExistingKey(json)
                        select (json, nonExistingKey);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, nonExistingKey) = x;

            // Act
            var action = () => json.GetProperty(nonExistingKey);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetProperty_throws_if_the_property_is_null()
    {
        var generator = from json in Generator.JsonObject.Where(HasNullProperty)
                        from kvp in Gen.Elements(json.Where(HasNullProperty))
                        select (json, kvp.Key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, keyWithNullValue) = x;

            // Act
            var action = () => json.GetProperty(keyWithNullValue);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetProperty_returns_the_existing_property()
    {
        var generator = from json in Generator.JsonObject.Where(HasNonNullProperty)
                        from kvp in Gen.Elements(json.Where(HasNonNullProperty))
                        select (json, kvp.Key, kvp.Value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetProperty(key);

            // Assert
            JsonNode.DeepEquals(value, expectedValue).Should().BeTrue();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetOptionalProperty_returns_None_if_the_property_does_not_exist()
    {
        var generator = from json in Generator.JsonObject
                        from nonExistingKey in GenerateNonExistingKey(json)
                        select (json, nonExistingKey);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, nonExistingKey) = x;

            // Act
            var option = json.GetOptionalProperty(nonExistingKey);

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetOptionalProperty_returns_None_if_the_property_is_null()
    {
        var generator = from json in Generator.JsonObject.Where(HasNullProperty)
                        from kvp in Gen.Elements(json.Where(HasNullProperty))
                        select (json, kvp.Key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, keyWithNullValue) = x;

            // Act
            var option = json.GetOptionalProperty(keyWithNullValue);

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetOptionalProperty_returns_Some_if_the_property_exists()
    {
        var generator = from json in Generator.JsonObject.Where(HasNonNullProperty)
                        from kvp in Gen.Elements(json.Where(HasNonNullProperty))
                        select (json, kvp.Key, kvp.Value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var option = json.GetOptionalProperty(key);

            // Assert
            option.Should().BeSome(value => JsonNode.DeepEquals(value, expectedValue).Should().BeTrue());
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetProperty_returns_left_if_the_property_does_not_exist()
    {
        var generator = from json in Generator.JsonObject
                        from nonExistingKey in GenerateNonExistingKey(json)
                        select (json, nonExistingKey);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, nonExistingKey) = x;

            // Act
            var either = json.TryGetProperty(nonExistingKey);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetProperty_returns_left_if_the_property_is_null()
    {
        var generator = from json in Generator.JsonObject.Where(HasNullProperty)
                        from kvp in Gen.Elements(json.Where(HasNullProperty))
                        select (json, kvp.Key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, keyWithNullValue) = x;

            // Act
            var either = json.TryGetProperty(keyWithNullValue);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetProperty_returns_right_if_the_property_exists()
    {
        var generator = from json in Generator.JsonObject.Where(HasNonNullProperty)
                        from kvp in Gen.Elements(json.Where(HasNonNullProperty))
                        select (json, kvp.Key, kvp.Value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetProperty(key);

            // Assert
            either.Should().BeRight(value => JsonNode.DeepEquals(value, expectedValue).Should().BeTrue());
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetJsonObjectProperty_throws_if_the_property_is_not_a_json_object()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonJsonObjectKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetJsonObjectProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetJsonObjectProperty_returns_value_if_the_property_is_a_json_object()
    {
        var generator = from json in Generator.JsonObject.Where(HasJsonObjectProperty)
                        from kvp in Gen.Elements(json.Where(HasJsonObjectProperty))
                        select (json, kvp.Key, kvp.Value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetJsonObjectProperty(key);

            // Assert
            JsonNode.DeepEquals(value, expectedValue).Should().BeTrue();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetJsonObjectProperty_returns_left_if_the_property_is_not_a_json_object()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonJsonObjectKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetJsonObjectProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetJsonObjectProperty_returns_right_if_the_property_is_a_json_object()
    {
        var generator = from json in Generator.JsonObject.Where(HasJsonObjectProperty)
                        from kvp in Gen.Elements(json.Where(HasJsonObjectProperty))
                        select (json, kvp.Key, kvp.Value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetJsonObjectProperty(key);

            // Assert
            either.Should().BeRight(value => JsonNode.DeepEquals(value, expectedValue).Should().BeTrue());
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetJsonArrayProperty_throws_if_the_property_is_not_a_json_object()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonJsonArrayKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetJsonArrayProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetJsonArrayProperty_returns_value_if_the_property_is_a_json_array()
    {
        var generator = from json in Generator.JsonObject.Where(HasJsonArrayProperty)
                        from kvp in Gen.Elements(json.Where(HasJsonArrayProperty))
                        select (json, kvp.Key, kvp.Value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetJsonArrayProperty(key);

            // Assert
            JsonNode.DeepEquals(value, expectedValue).Should().BeTrue();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetJsonArrayProperty_returns_left_if_the_property_is_not_a_json_array()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonJsonArrayKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetJsonArrayProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetJsonArrayProperty_returns_right_if_the_property_is_a_json_array()
    {
        var generator = from json in Generator.JsonObject.Where(HasJsonArrayProperty)
                        from kvp in Gen.Elements(json.Where(HasJsonArrayProperty))
                        select (json, kvp.Key, kvp.Value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetJsonArrayProperty(key);

            // Assert
            either.Should().BeRight(value => JsonNode.DeepEquals(value, expectedValue).Should().BeTrue());
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetJsonValueProperty_throws_if_the_property_is_not_a_json_value()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonJsonValueKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetJsonValueProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetJsonValueProperty_returns_value_if_the_property_is_a_json_value()
    {
        var generator = from json in Generator.JsonObject.Where(HasJsonValueProperty)
                        from kvp in Gen.Elements(json.Where(HasJsonValueProperty))
                        select (json, kvp.Key, kvp.Value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetJsonValueProperty(key);

            // Assert
            JsonNode.DeepEquals(value, expectedValue).Should().BeTrue();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetJsonValueProperty_returns_left_if_the_property_is_not_a_json_value()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonJsonValueKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetJsonValueProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetJsonValueProperty_returns_right_if_the_property_is_a_json_value()
    {
        var generator = from json in Generator.JsonObject.Where(HasJsonValueProperty)
                        from kvp in Gen.Elements(json.Where(HasJsonValueProperty))
                        select (json, kvp.Key, kvp.Value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetJsonValueProperty(key);

            // Assert
            either.Should().BeRight(value => JsonNode.DeepEquals(value, expectedValue).Should().BeTrue());
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetStringProperty_throws_if_the_property_is_not_a_string()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonStringKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetStringProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetStringProperty_returns_value_if_the_property_is_a_string()
    {
        var generator = from json in Generator.JsonObject.Where(HasStringProperty)
                        from kvp in Gen.Elements(json.Where(HasStringProperty))
                        select (json, kvp.Key, kvp.Value.TryAsString().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetStringProperty(key);

            // Assert
            value.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetStringProperty_returns_left_if_the_property_is_not_a_string()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonStringKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetStringProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetStringProperty_returns_right_if_the_property_is_a_string()
    {
        var generator = from json in Generator.JsonObject.Where(HasStringProperty)
                        from kvp in Gen.Elements(json.Where(HasStringProperty))
                        select (json, kvp.Key, kvp.Value.TryAsString().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetStringProperty(key);

            // Assert
            either.Should().BeRight().Which.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetNonEmptyOrWhiteSpaceStringProperty_throws_if_the_property_is_not_a_non_empty_or_whitespace_string()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonNonEmptyOrWhiteSpaceStringKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetNonEmptyOrWhiteSpaceStringProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetNonEmptyOrWhiteSpaceStringProperty_returns_value_if_the_property_is_a_non_empty_or_whitespace_string()
    {
        var generator = from json in Generator.JsonObject.Where(HasNonEmptyOrWhiteSpaceStringProperty)
                        from kvp in Gen.Elements(json.Where(HasNonEmptyOrWhiteSpaceStringProperty))
                        select (json, kvp.Key, kvp.Value.TryAsString().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetNonEmptyOrWhiteSpaceStringProperty(key);

            // Assert
            value.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetNonEmptyOrWhiteSpaceStringProperty_returns_left_if_the_property_is_not_a_non_empty_or_whitespace_string()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonNonEmptyOrWhiteSpaceStringKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetNonEmptyOrWhiteSpaceStringProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetNonEmptyOrWhiteSpaceStringProperty_returns_right_if_the_property_is_a_non_empty_or_whitespace_string()
    {
        var generator = from json in Generator.JsonObject.Where(HasNonEmptyOrWhiteSpaceStringProperty)
                        from kvp in Gen.Elements(json.Where(HasNonEmptyOrWhiteSpaceStringProperty))
                        select (json, kvp.Key, kvp.Value.TryAsString().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetNonEmptyOrWhiteSpaceStringProperty(key);

            // Assert
            either.Should().BeRight().Which.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetGuidProperty_throws_if_the_property_is_not_a_GUID()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonGuidKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetGuidProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetGuidProperty_returns_value_if_the_property_is_a_GUID()
    {
        var generator = from json in Generator.JsonObject.Where(HasGuidProperty)
                        from kvp in Gen.Elements(json.Where(HasGuidProperty))
                        select (json, kvp.Key, kvp.Value.TryAsGuid().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetGuidProperty(key);

            // Assert
            value.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetGuidProperty_returns_left_if_the_property_is_not_a_GUID()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonGuidKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetGuidProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetGuidProperty_returns_right_if_the_property_is_a_GUID()
    {
        var generator = from json in Generator.JsonObject.Where(HasGuidProperty)
                        from kvp in Gen.Elements(json.Where(HasGuidProperty))
                        select (json, kvp.Key, kvp.Value.TryAsGuid().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetGuidProperty(key);

            // Assert
            either.Should().BeRight().Which.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetAbsoluteUriProperty_throws_if_the_property_is_not_an_absolute_uri()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonAbsoluteUriKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetAbsoluteUriProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetAbsoluteUriProperty_returns_value_if_the_property_is_an_absolute_uri()
    {
        var generator = from json in Generator.JsonObject.Where(HasAbsoluteUriProperty)
                        from kvp in Gen.Elements(json.Where(HasAbsoluteUriProperty))
                        select (json, kvp.Key, kvp.Value.TryAsAbsoluteUri().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetAbsoluteUriProperty(key);

            // Assert
            value.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetAbsoluteUriProperty_returns_left_if_the_property_is_not_an_absolute_uri()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonAbsoluteUriKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetAbsoluteUriProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetAbsoluteUriProperty_returns_right_if_the_property_is_an_absolute_uri()
    {
        var generator = from json in Generator.JsonObject.Where(HasAbsoluteUriProperty)
                        from kvp in Gen.Elements(json.Where(HasAbsoluteUriProperty))
                        select (json, kvp.Key, kvp.Value.TryAsAbsoluteUri().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetAbsoluteUriProperty(key);

            // Assert
            either.Should().BeRight().Which.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetDateTimeOffsetProperty_throws_if_the_property_is_not_a_DateTimeOffset()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonDateTimeOffsetKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetDateTimeOffsetProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetDateTimeOffsetProperty_returns_value_if_the_property_is_a_DateTimeOffset()
    {
        var generator = from json in Generator.JsonObject.Where(HasDateTimeOffsetProperty)
                        from kvp in Gen.Elements(json.Where(HasDateTimeOffsetProperty))
                        select (json, kvp.Key, kvp.Value.TryAsDateTimeOffset().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetDateTimeOffsetProperty(key);

            // Assert
            expectedValue.Should().Be(value);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetDateTimeOffsetProperty_returns_left_if_the_property_is_not_a_DateTimeOffset()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonDateTimeOffsetKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetDateTimeOffsetProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetDateTimeOffsetProperty_returns_right_if_the_property_is_a_DateTimeOffset()
    {
        var generator = from json in Generator.JsonObject.Where(HasDateTimeOffsetProperty)
                        from kvp in Gen.Elements(json.Where(HasDateTimeOffsetProperty))
                        select (json, kvp.Key, kvp.Value.TryAsDateTimeOffset().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetDateTimeOffsetProperty(key);

            // Assert
            either.Should().BeRight().Which.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetDateTimeProperty_throws_if_the_property_is_not_a_DateTime()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonDateTimeKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetDateTimeProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetDateTimeProperty_returns_value_if_the_property_is_a_DateTime()
    {
        var generator = from json in Generator.JsonObject.Where(HasDateTimeProperty)
                        from kvp in Gen.Elements(json.Where(HasDateTimeProperty))
                        select (json, kvp.Key, kvp.Value.TryAsDateTime().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetDateTimeProperty(key);

            // Assert
            expectedValue.Should().Be(value);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetDateTimeProperty_returns_left_if_the_property_is_not_a_DateTime()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonDateTimeKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetDateTimeProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetDateTimeProperty_returns_right_if_the_property_is_a_DateTime()
    {
        var generator = from json in Generator.JsonObject.Where(HasDateTimeProperty)
                        from kvp in Gen.Elements(json.Where(HasDateTimeProperty))
                        select (json, kvp.Key, kvp.Value.TryAsDateTime().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetDateTimeProperty(key);

            // Assert
            either.Should().BeRight().Which.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetIntProperty_throws_if_the_property_is_not_an_int()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonIntKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetIntProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetIntProperty_returns_value_if_the_property_is_an_int()
    {
        var generator = from json in Generator.JsonObject.Where(HasIntProperty)
                        from kvp in Gen.Elements(json.Where(HasIntProperty))
                        select (json, kvp.Key, kvp.Value.TryAsInt().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetIntProperty(key);

            // Assert
            expectedValue.Should().Be(value);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetIntProperty_returns_left_if_the_property_is_not_an_int()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonIntKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetIntProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetIntProperty_returns_right_if_the_property_is_an_int()
    {
        var generator = from json in Generator.JsonObject.Where(HasIntProperty)
                        from kvp in Gen.Elements(json.Where(HasIntProperty))
                        select (json, kvp.Key, kvp.Value.TryAsInt().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetIntProperty(key);

            // Assert
            either.Should().BeRight().Which.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetDoubleProperty_throws_if_the_property_is_not_a_double()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonDoubleKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetDoubleProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetDoubleProperty_returns_value_if_the_property_is_a_double()
    {
        var generator = from json in Generator.JsonObject.Where(HasDoubleProperty)
                        from kvp in Gen.Elements(json.Where(HasDoubleProperty))
                        select (json, kvp.Key, kvp.Value.TryAsDouble().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetDoubleProperty(key);

            // Assert
            expectedValue.Should().Be(value);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetDoubleProperty_returns_left_if_the_property_is_not_a_double()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonDoubleKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetDoubleProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetDoubleProperty_returns_right_if_the_property_is_a_double()
    {
        var generator = from json in Generator.JsonObject.Where(HasDoubleProperty)
                        from kvp in Gen.Elements(json.Where(HasDoubleProperty))
                        select (json, kvp.Key, kvp.Value.TryAsDouble().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetDoubleProperty(key);

            // Assert
            either.Should().BeRight().Which.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetBoolProperty_throws_if_the_property_is_not_a_bool()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonBoolKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var action = () => json.GetBoolProperty(key);

            // Assert
            action.Should().Throw<JsonException>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetBoolProperty_returns_value_if_the_property_is_a_bool()
    {
        var generator = from json in Generator.JsonObject.Where(HasBoolProperty)
                        from kvp in Gen.Elements(json.Where(HasBoolProperty))
                        select (json, kvp.Key, kvp.Value.TryAsBool().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var value = json.GetBoolProperty(key);

            // Assert
            expectedValue.Should().Be(value);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetBoolProperty_returns_left_if_the_property_is_not_a_bool()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonBoolKey(json)
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var either = json.TryGetBoolProperty(key);

            // Assert
            either.Should().BeLeft();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetBoolProperty_returns_right_if_the_property_is_a_bool()
    {
        var generator = from json in Generator.JsonObject.Where(HasBoolProperty)
                        from kvp in Gen.Elements(json.Where(HasBoolProperty))
                        select (json, kvp.Key, kvp.Value.TryAsBool().ValueUnsafe());

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, expectedValue) = x;

            // Act
            var either = json.TryGetBoolProperty(key);

            // Assert
            either.Should().BeRight().Which.Should().Be(expectedValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property SetProperty_sets_the_property()
    {
        var generator = from json in Generator.JsonObject
                        from key in Generator.NonEmptyOrWhiteSpaceString
                        from value in Generator.JsonNode
                        select (json, key, value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, value) = x;

            // Act
            var result = json.SetProperty(key, value);

            // Assert
            result.Should().ContainKey(key).WhoseValue.Should().Be(value);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property SetPropertyIfNotNull_sets_the_property_if_it_is_not_null()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonExistingKey(json)
                        from value in Generator.JsonNode.OrNull()
                        select (json, key, value);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, value) = x;

            // Act
            var result = json.SetPropertyIfNotNull(key, value);

            // Assert
            if (value is null)
            {
                result.Should().NotContainKey(key);
            }
            else
            {
                result.Should().ContainKey(key).WhoseValue.Should().Be(value);
            }
        });
    }

    [FsCheck.NUnit.Property()]
    public Property SetPropertyIfSome_sets_the_property_if_it_is_some()
    {
        var generator = from json in Generator.JsonObject
                        from key in GenerateNonExistingKey(json)
                        from option in Generator.JsonNode.OptionOf()
                        select (json, key, option);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key, option) = x;

            // Act
            var result = json.SetPropertyIfSome(key, option);

            // Assert
            option.Match(value => result.Should().ContainKey(key).WhoseValue.Should().Be(value),
                         () => result.Should().NotContainKey(key));
        });
    }

    [FsCheck.NUnit.Property()]
    public Property RemoveProperty_removes_the_property()
    {
        var generator = from json in Generator.JsonObject
                        from key in from randomKeys in Generator.AlphaNumericString.NonEmptySeqOf()
                                    let keys = json.ToDictionary().Keys.Append(randomKeys)
                                    from selectedKey in Gen.Elements(keys)
                                    select selectedKey
                        select (json, key);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (json, key) = x;

            // Act
            var result = json.RemoveProperty(key);

            // Assert
            result.Should().NotContainKey(key);
        });
    }

    private static Gen<string> GenerateNonExistingKey(JsonObject jsonObject) =>
        Generator.NonEmptyOrWhiteSpaceString
                 .Where(key => jsonObject.TryGetPropertyValue(key, out var _) is false);

    private static bool HasNullProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasNullProperty);

    private static bool HasNullProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value is null;

    private static bool HasNonNullProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasNonNullProperty);

    private static bool HasNonNullProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value is not null;

    private static Gen<string> GenerateNonJsonObjectKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonJsonObjectProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonJsonObjectProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsJsonObject().IsNone;

    private static bool HasJsonObjectProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasJsonObjectProperty);

    private static bool HasJsonObjectProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsJsonObject().IsSome;

    private static Gen<string> GenerateNonJsonArrayKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonJsonArrayProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonJsonArrayProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsJsonArray().IsNone;

    private static bool HasJsonArrayProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasJsonArrayProperty);

    private static bool HasJsonArrayProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsJsonArray().IsSome;

    private static Gen<string> GenerateNonJsonValueKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonJsonValueProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonJsonValueProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsJsonValue().IsNone;

    private static bool HasJsonValueProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasJsonValueProperty);

    private static bool HasJsonValueProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsJsonValue().IsSome;

    private static Gen<string> GenerateNonStringKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonStringProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonStringProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsString().IsNone;

    private static bool HasStringProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasStringProperty);

    private static bool HasStringProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsString().IsSome;

    private static Gen<string> GenerateNonNonEmptyOrWhiteSpaceStringKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonNonEmptyOrWhiteSpaceStringProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonNonEmptyOrWhiteSpaceStringProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsString()
                          .Where(value => string.IsNullOrWhiteSpace(value) is false).IsNone;

    private static bool HasNonEmptyOrWhiteSpaceStringProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasNonEmptyOrWhiteSpaceStringProperty);

    private static bool HasNonEmptyOrWhiteSpaceStringProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsString()
                          .Where(value => string.IsNullOrWhiteSpace(value) is false).IsSome;

    private static Gen<string> GenerateNonGuidKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonGuidProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonGuidProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsGuid().IsNone;

    private static bool HasGuidProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasGuidProperty);

    private static bool HasGuidProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsGuid().IsSome;

    private static Gen<string> GenerateNonAbsoluteUriKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonAbsoluteUriProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonAbsoluteUriProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsAbsoluteUri().IsNone;

    private static bool HasAbsoluteUriProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasAbsoluteUriProperty);

    private static bool HasAbsoluteUriProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsAbsoluteUri().IsSome;

    private static Gen<string> GenerateNonDateTimeOffsetKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonDateTimeOffsetProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonDateTimeOffsetProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsDateTimeOffset().IsNone;

    private static bool HasDateTimeOffsetProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasDateTimeOffsetProperty);

    private static bool HasDateTimeOffsetProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsDateTimeOffset().IsSome;

    private static Gen<string> GenerateNonDateTimeKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonDateTimeProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonDateTimeProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsDateTime().IsNone;

    private static bool HasDateTimeProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasDateTimeProperty);

    private static bool HasDateTimeProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsDateTime().IsSome;

    private static Gen<string> GenerateNonIntKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonIntProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonIntProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsInt().IsNone;

    private static bool HasIntProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasIntProperty);

    private static bool HasIntProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsInt().IsSome;

    private static Gen<string> GenerateNonDoubleKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonDoubleProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonDoubleProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsDouble().IsNone;

    private static bool HasDoubleProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasDoubleProperty);

    private static bool HasDoubleProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsDouble().IsSome;

    private static Gen<string> GenerateNonBoolKey(JsonObject jsonObject)
    {
        var keys = jsonObject.Where(HasNonBoolProperty)
                             .Map(kvp => kvp.Key);

        return keys.Any()
                ? Gen.Elements(keys)
                : GenerateNonExistingKey(jsonObject);
    }

    private static bool HasNonBoolProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsBool().IsNone;

    private static bool HasBoolProperty(JsonObject jsonObject) =>
        jsonObject.Any(HasBoolProperty);

    private static bool HasBoolProperty(KeyValuePair<string, JsonNode?> keyValuePair) =>
        keyValuePair.Value.TryAsBool().IsSome;
}