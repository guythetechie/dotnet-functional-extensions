using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using NUnit.Framework;
using System;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class EitherExtensionsTests
{
    [FsCheck.NUnit.Property()]
    public Property IfLeftThrow_returns_value_if_either_is_right()
    {
        var arbitrary = Generator.GenerateDefault<object>()
                                 .Where(x => x is not null)
                                 .ToArbitrary();

        return Prop.ForAll(arbitrary, value =>
        {
            // Arrange
            var either = Either<string, object>.Right(value);

            // Act
            var result = either.IfLeftThrow();

            // Assert
            result.Should().Be(value);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property IfLeftThrow_throws_if_either_is_left()
    {
        var arbitrary = Generator.AlphaNumericString.ToArbitrary();

        return Prop.ForAll(arbitrary, errorMessage =>
        {
            // Arrange
            var either = Either<string, object>.Left(errorMessage);

            // Act
            var action = () => either.IfLeftThrow();

            // Assert
            action.Should().Throw<Exception>().WithMessage(errorMessage);
        });
    }
}