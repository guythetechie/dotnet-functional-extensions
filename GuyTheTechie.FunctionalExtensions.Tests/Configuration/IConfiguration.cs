using FluentAssertions;
using FluentAssertions.LanguageExt;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class IConfigurationTests
{
    [FsCheck.NUnit.Property()]
    public Property GetValue_throws_if_configuration_key_does_not_exist()
    {
        var generator = from items in GenerateConfigurationItems()
                        let keys = items.Keys.ToSeq()
                        from nonExistingKey in Generator.NonEmptyOrWhiteSpaceString
                                                        .Where(key => keys.Contains(key, StringComparer.OrdinalIgnoreCase) is false)
                        select (items, nonExistingKey);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (items, nonExistingKey) = x;
            var configuration = ToConfiguration(items);

            // Act
            var action = () => configuration.GetValue(nonExistingKey);

            // Assert
            action.Should().Throw<Exception>();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetValue_returns_None_if_configuration_key_does_not_exist()
    {
        var generator = from items in GenerateConfigurationItems()
                        let keys = items.Keys.ToSeq()
                        from nonExistingKey in Generator.NonEmptyOrWhiteSpaceString
                                                        .Where(key => keys.Contains(key, StringComparer.OrdinalIgnoreCase) is false)
                        select (items, nonExistingKey);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (items, nonExistingKey) = x;
            var configuration = ToConfiguration(items);

            // Act
            var result = configuration.TryGetValue(nonExistingKey);

            // Assert
            result.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetValue_returns_None_if_configuration_value_is_null()
    {
        var generator = from items in GenerateNonEmptyConfigurationItems()
                        from keyWithNullValue in Gen.Elements(items.Keys)
                        let itemsWithNullValue = items.SetItem(keyWithNullValue, null as string)
                        select (itemsWithNullValue, keyWithNullValue);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (items, keyWithNullValue) = x;
            var configuration = ToConfiguration(items);

            // Act
            var result = configuration.TryGetValue(keyWithNullValue);

            // Assert
            result.Should().BeNone();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property TryGetValue_returns_the_value_if_configuration_key_has_a_non_null_value()
    {
        var generator = from items in GenerateNonEmptyConfigurationItems().Where(items => items.Values.Any(value => value is not null))
                        from existingKey in Gen.Elements(items.Keys).Where(key => items[key] is not null)
                        select (items, existingKey);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (items, existingKey) = x;
            var configuration = ToConfiguration(items);

            // Act
            var result = configuration.TryGetValue(existingKey);

            // Assert
            var expectedValue = items[existingKey];
            result.Should().BeSome(value => value.Should().Be(expectedValue));
        });
    }

    private static Gen<HashMap<string, string?>> GenerateNonEmptyConfigurationItems() =>
        GenerateConfigurationItems().Where(items => items.IsEmpty is false);

    private static Gen<HashMap<string, string?>> GenerateConfigurationItems() =>
        Gen.Zip(Generator.AlphaNumericString, GenerateNullableString())
           .SeqOf()
           .DistinctBy(x => x.Item1.ToUpperInvariant())
           .Select(x => x.ToHashMap());

    private static Gen<string?> GenerateNullableString() =>
        Generator.GenerateDefault<string?>().OrNull();

    private static IConfiguration ToConfiguration(IEnumerable<(string, string?)> items)
    {
        var keyValuePairs = items.Map(x => KeyValuePair.Create(x.Item1, x.Item2));

        return new ConfigurationBuilder()
                    .AddInMemoryCollection(keyValuePairs)
                    .Build();
    }
}