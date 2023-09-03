using FluentAssertions;
using FluentAssertions.LanguageExt;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class IAsyncEnumerableTests
{
    [FsCheck.NUnit.Property()]
    public Property Do_executes_the_action_when_iterating()
    {
        var generator = from items in Generator.GenerateDefault<object>().SeqOf()
                        from iterations in Gen.Choose(0, items.Count)
                        select (items, iterations);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, async x =>
        {
            // Arrange
            var (items, iterations) = x;
            var addedItems = AtomSeq<object>.Empty;
            void action(object obj) => addedItems.Add(obj);

            // Act
            await items.ToAsyncEnumerable()
                       .Do(action)
                       .Take(iterations)
                       .ToArrayAsync(CancellationToken.None);

            // Assert
            addedItems.Should().BeEquivalentTo(items.Take(iterations));
        });
    }

    [FsCheck.NUnit.Property()]
    public Property Iter_executes_action()
    {
        var arbitrary = Generator.GenerateDefault<object>()
                                 .SeqOf()
                                 .Select(x => x.ToAsyncEnumerable())
                                 .ToArbitrary();

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
            var itemCount = await items.CountAsync();
            counter.Should().Be(itemCount);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property IterParallel_executes_action()
    {
        var arbitrary = Generator.GenerateDefault<object>()
                                 .SeqOf()
                                 .Select(x => x.ToAsyncEnumerable())
                                 .ToArbitrary();

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
            var itemCount = await items.CountAsync();
            counter.Should().Be(itemCount);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property ToSeq_returns_seq_of_enumerable()
    {
        var arbitrary = Generator.GenerateDefault<object>()
                                 .SeqOf()
                                 .ToArbitrary();

        return Prop.ForAll(arbitrary, async items =>
        {
            // Arrange
            var enumerable = items.ToAsyncEnumerable();

            // Act
            var seq = await enumerable.ToSeq(CancellationToken.None);

            // Assert
            seq.Should().BeEquivalentTo(items);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property Choose_returns_somes()
    {
        var generator = from objects in Generator.GenerateDefault<object>().SeqOf()
                        from f in Generator.GenerateDefault<Func<object, bool>>()
                        let chooser = (Func<object, Option<object>>)
                                      ((object obj) => f(obj)
                                                        ? Option<object>.Some(obj)
                                                        : Option<object>.None)
                        select (objects, chooser);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, async x =>
        {
            // Arrange
            var (objects, chooser) = x;

            // Act
            var somes = await objects.ToAsyncEnumerable()
                                     .Choose(chooser)
                                     .ToSeq(CancellationToken.None);

            // Assert
            var expected = objects.Where(x => chooser(x).IsSome);
            somes.Should().BeEquivalentTo(expected);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property Choose_with_async_returns_somes()
    {
        var generator = from objects in Generator.GenerateDefault<object>().SeqOf()
                        from f in Generator.GenerateDefault<Func<object, bool>>()
                        let chooser = (Func<object, Option<object>>)
                                      ((object obj) => f(obj)
                                                        ? Option<object>.Some(obj)
                                                        : Option<object>.None)
                        select (objects, chooser);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, async x =>
        {
            // Arrange
            var (objects, chooser) = x;

            // Act
            var somes = await objects.ToAsyncEnumerable()
                                     .Choose(async obj => await ValueTask.FromResult(chooser(obj)))
                                     .ToSeq(CancellationToken.None);

            // Assert
            var expected = objects.Where(x => chooser(x).IsSome);
            somes.Should().BeEquivalentTo(expected);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property HeadOrNone_returns_the_first_item()
    {
        var arbitrary = Generator.GenerateDefault<object>()
                                 .NonEmptySeqOf()
                                 .ToArbitrary();

        return Prop.ForAll(arbitrary, async items =>
        {
            // Arrange
            var enumerable = items.ToAsyncEnumerable();

            // Act
            var option = await enumerable.HeadOrNone(CancellationToken.None);

            // Assert
            var expected = items.Head;
            option.Should().BeSome().Which.Should().Be(expected);
        });
    }

    [Test]
    public async Task HeadOrNone_returns_None_if_the_enumerable_is_empty()
    {
        // Arrange
        var enumerable = Enumerable.Empty<object>()
                                   .ToAsyncEnumerable();

        // Act
        var option = await enumerable.HeadOrNone(CancellationToken.None);

        // Assert
        option.Should().BeNone();
    }
}
