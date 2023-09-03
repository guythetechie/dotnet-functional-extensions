using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using NUnit.Framework;
using System;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class OptionExtensionsTests
{
    [FsCheck.NUnit.Property()]
    public Property IfNoneThrow_returns_value_if_option_is_Some()
    {
        var arbitrary = Generator.GenerateDefault<object>()
                                 .Where(x => x is not null)
                                 .ToArbitrary();

        return Prop.ForAll(arbitrary, value =>
        {
            // Arrange
            var option = Option<object>.Some(value);

            // Act
            var result = option.IfNoneThrow(string.Empty);

            // Assert
            result.Should().Be(value);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property IfNoneThrow_throws_if_option_is_None()
    {
        var arbitrary = Generator.AlphaNumericString.ToArbitrary();

        return Prop.ForAll(arbitrary, errorMessage =>
        {
            // Arrange
            var option = Option<int>.None;

            // Act
            var action = () => option.IfNoneThrow(errorMessage);

            // Assert
            action.Should().Throw<Exception>().WithMessage(errorMessage);
        });
    }
}