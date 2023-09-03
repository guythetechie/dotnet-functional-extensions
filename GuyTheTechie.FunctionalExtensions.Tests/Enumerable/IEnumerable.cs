using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class IEnumerableTests
{
    [FsCheck.NUnit.Property()]
    public Property Iter_executes_action()
    {
        var generator = Generator.GenerateDefault<object>().ListOf();
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, async items =>
        {
            // Arrange
            var counter = 0;
            async ValueTask action(object _)
            {
                await ValueTask.CompletedTask;
                Interlocked.Increment(ref counter);
            };

            // Act
            await items.Iter(action, CancellationToken.None);

            // Assert
            counter.Should().Be(items.Count);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property IterParallel_executes_action()
    {
        var generator = Generator.GenerateDefault<object>().ListOf();
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, async items =>
        {
            // Arrange
            var counter = 0;
            async ValueTask action(object _)
            {
                await ValueTask.CompletedTask;
                Interlocked.Increment(ref counter);
            };

            // Act
            await items.IterParallel(action, CancellationToken.None);

            // Assert
            counter.Should().Be(items.Count);
        });
    }
}
