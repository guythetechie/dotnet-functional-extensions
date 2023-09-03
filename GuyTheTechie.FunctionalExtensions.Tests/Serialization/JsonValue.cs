using FluentAssertions;
using FluentAssertions.LanguageExt;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.Json.Nodes;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class JsonValueTests
{
    [FsCheck.NUnit.Property()]
    public Property TryAsString_returns_none_if_the_json_value_is_not_a_string()
    {
        var generator = Generator.JsonValue
                                 .Where(value => value.TryGetValue<string>(out var _) is false);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonValue =>
        {
            // Act
            var option = jsonValue.TryAsString();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsString_returns_some_if_the_json_value_is_a_string()
    {
        var generator = from stringValue in Generator.GenerateDefault<string>()
                        let jsonValue = JsonValue.Create(stringValue)
                        select (jsonValue, stringValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonValue, stringValue) = x;

            // Act
            var option = jsonValue.TryAsString();

            // Assert
            option.Should().BeSome().Which.Should().Be(stringValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsGuid_returns_none_if_the_json_value_is_not_a_guid()
    {
        var generator = Generator.JsonValue
                                 .Where(value => value.TryGetValue<Guid>(out var _) is false
                                                 && value.TryGetValue<string>(out var stringValue) is false
                                                 && Guid.TryParse(stringValue, out var _) is false);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonValue =>
        {
            // Act
            var option = jsonValue.TryAsGuid();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsGuid_returns_some_if_the_json_value_is_a_guid()
    {
        var generator = from guidValue in Generator.GenerateDefault<Guid>()
                        from jsonValue in Gen.Elements(JsonValue.Create(guidValue), JsonValue.Create(guidValue.ToString()))
                        select (jsonValue, guidValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonValue, guidValue) = x;

            // Act
            var option = jsonValue.TryAsGuid();

            // Assert
            option.Should().BeSome().Which.Should().Be(guidValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsAbsoluteUri_returns_none_if_the_json_value_is_not_an_absolute_uri()
    {
        var generator = Generator.JsonValue
                                 .Where(value => value.TryGetValue<string>(out var stringValue) is false
                                                 && Uri.TryCreate(stringValue, UriKind.Absolute, out var _) is false);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonValue =>
        {
            // Act
            var option = jsonValue.TryAsAbsoluteUri();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsAbsoluteUri_returns_some_if_the_json_value_is_an_absolute_uri()
    {
        var generator = from uri in Generator.AbsoluteUri
                        let jsonValue = JsonValue.Create(uri.ToString())
                        select (jsonValue, uri);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonValue, uri) = x;

            // Act
            var option = jsonValue.TryAsAbsoluteUri();

            // Assert
            option.Should().BeSome().Which.Should().Be(uri);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDateTimeOffset_returns_none_if_the_json_value_is_not_a_DateTimeOffset()
    {
        var generator = Generator.JsonValue
                                 .Where(value => value.TryGetValue<DateTimeOffset>(out var _) is false
                                                 && value.TryGetValue<string>(out var stringValue) is false
                                                 && DateTimeOffset.TryParse(stringValue, out var _) is false);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonValue =>
        {
            // Act
            var option = jsonValue.TryAsDateTimeOffset();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDateTimeOffset_returns_some_if_the_json_value_is_a_DateTimeOffset()
    {
        var generator = from dateTimeOffsetValue in Generator.GenerateDefault<DateTimeOffset>()
                        from jsonValue in Gen.Elements(JsonValue.Create(dateTimeOffsetValue), JsonValue.Create(dateTimeOffsetValue.ToString("O")))
                        select (jsonValue, dateTimeOffsetValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonValue, dateTimeOffsetValue) = x;

            // Act
            var option = jsonValue.TryAsDateTimeOffset();

            // Assert
            option.Should().BeSome().Which.Should().BeCloseTo(dateTimeOffsetValue, TimeSpan.FromSeconds(1));
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDateTime_returns_none_if_the_json_value_is_not_a_DateTime()
    {
        var generator = Generator.JsonValue
                                 .Where(value => value.TryGetValue<DateTime>(out var _) is false
                                                 && value.TryGetValue<string>(out var stringValue) is false
                                                 && DateTime.TryParse(stringValue, out var _) is false);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonValue =>
        {
            // Act
            var option = jsonValue.TryAsDateTime();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDateTime_returns_some_if_the_json_value_is_a_DateTime()
    {
        var generator = from dateTimeValue in Generator.GenerateDefault<DateTime>()
                        from jsonValue in Gen.Elements(JsonValue.Create(dateTimeValue), JsonValue.Create(dateTimeValue.ToString("s")))
                        select (jsonValue, dateTimeValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonValue, dateTimeValue) = x;

            // Act
            var option = jsonValue.TryAsDateTime();

            // Assert
            option.Should().BeSome().Which.Should().BeCloseTo(dateTimeValue, TimeSpan.FromSeconds(1));
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsInt_returns_none_if_the_json_value_is_not_an_int()
    {
        var generator = Generator.JsonValue
                                 .Where(value => value.TryGetValue<int>(out var _) is false);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonValue =>
        {
            // Act
            var option = jsonValue.TryAsInt();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsInt_returns_some_if_the_json_value_is_an_int()
    {
        var generator = from intValue in Generator.GenerateDefault<int>()
                        let jsonValue = JsonValue.Create(intValue)
                        select (jsonValue, intValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonValue, intValue) = x;

            // Act
            var option = jsonValue.TryAsInt();

            // Assert
            option.Should().BeSome().Which.Should().Be(intValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDouble_returns_none_if_the_json_value_is_not_a_double()
    {
        var generator = Generator.JsonValue
                                 .Where(value => value.TryGetValue<double>(out var _) is false);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonValue =>
        {
            // Act
            var option = jsonValue.TryAsDouble();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsDouble_returns_some_if_the_json_value_is_a_double()
    {
        var generator = from doubleValue in Generator.GenerateDefault<double>()
                                                     .Where(double.IsFinite)
                                                     .Where(d => double.IsNaN(d) is false)
                        let jsonValue = JsonValue.Create(doubleValue)
                        select (jsonValue, doubleValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonValue, doubleValue) = x;

            // Act
            var option = jsonValue.TryAsDouble();

            // Assert
            option.Should().BeSome().Which.Should().Be(doubleValue);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsBool_returns_none_if_the_json_value_is_not_a_bool()
    {
        var generator = Generator.JsonValue
                                 .Where(value => value.TryGetValue<bool>(out var _) is false);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, jsonValue =>
        {
            // Act
            var option = jsonValue.TryAsBool();

            // Assert
            option.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryAsBool_returns_some_if_the_json_value_is_a_bool()
    {
        var generator = from boolValue in Generator.GenerateDefault<bool>()
                        let jsonValue = JsonValue.Create(boolValue)
                        select (jsonValue, boolValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonValue, boolValue) = x;

            // Act
            var option = jsonValue.TryAsBool();

            // Assert
            option.Should().BeSome();
            option.ValueUnsafe().Should().Be(boolValue);
        });
    }
}