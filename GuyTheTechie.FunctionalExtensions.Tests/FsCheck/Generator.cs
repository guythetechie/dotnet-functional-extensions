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
using System.Threading;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class GeneratorTests
{
    [FsCheck.NUnit.Property()]
    public Property PositiveInteger_only_returns_positive_integers()
    {
        var arbitrary = Generator.PositiveInteger.ToArbitrary();

        return Prop.ForAll(arbitrary, item => item.Should().BePositive());
    }

    [FsCheck.NUnit.Property()]
    public Property NegativeInteger_only_returns_positive_integers()
    {
        var arbitrary = Generator.NegativeInteger.ToArbitrary();

        return Prop.ForAll(arbitrary, item => item.Should().BeNegative());
    }

    [FsCheck.NUnit.Property()]
    public Property AlphaNumericString_only_returns_alphanumeric_strings()
    {
        var arbitrary = Generator.AlphaNumericString.ToArbitrary();

        return Prop.ForAll(arbitrary,
                           item => item.AsEnumerable()
                                       .Should()
                                       .BeSubsetOf("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"));
    }

    [FsCheck.NUnit.Property()]
    public Property GuidString_returns_a_valid_guid()
    {
        var arbitrary = Generator.GuidString.ToArbitrary();

        return Prop.ForAll(arbitrary,
                           item => Guid.TryParse(item, out var _).Should().BeTrue());
    }

    [FsCheck.NUnit.Property()]
    public Property EmptyOrWhiteSpaceString_returns_empty_or_whitespace_strings()
    {
        var arbitrary = Generator.EmptyOrWhiteSpaceString.ToArbitrary();

        return Prop.ForAll(arbitrary,
                           item => string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item));
    }

    [FsCheck.NUnit.Property()]
    public Property NonEmptyOrWhiteSpaceString_does_not_return_empty_or_whitespace_strings()
    {
        var arbitrary = Generator.NonEmptyOrWhiteSpaceString.ToArbitrary();

        return Prop.ForAll(arbitrary,
                           item => item.Should().NotBeEmpty().And.NotBeNullOrWhiteSpace());
    }
}

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class GenExtensionsTests
{
    [FsCheck.NUnit.Property()]
    public Property SeqOf_does_not_throw()
    {
        var arbitrary = Generator.GenerateDefault<object>()
                                 .SeqOf()
                                 .ToArbitrary();

        return Prop.ForAll(arbitrary, _ => { });
    }

    [FsCheck.NUnit.Property()]
    public Property SeqOf_returns_a_seq_with_items_within_range()
    {
        var generator = from range in Generator.PositiveInteger.Two().Where(x => x.Item1 <= x.Item2)
                        let minimum = (uint)range.Item1
                        let maximum = (uint)range.Item2
                        from seq in Generator.GenerateDefault<object>().SeqOf(minimum, maximum)
                        select (seq, minimum, maximum);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            var (seq, minimum, maximum) = x;
            seq.Should()
               .HaveCountGreaterThanOrEqualTo((int)minimum)
               .And
               .HaveCountLessThanOrEqualTo((int)maximum);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property NonEmptySeqOf_is_not_empty()
    {
        var arbitrary = Generator.GenerateDefault<object>()
                                 .NonEmptySeqOf()
                                 .ToArbitrary();

        return Prop.ForAll(arbitrary, seq => seq.Should().NotBeEmpty());
    }

    [FsCheck.NUnit.Property()]
    public Property DistinctBy_only_returns_unique_items()
    {
        var generator = from function in Generator.GenerateDefault<Func<object, int>>()
                        from seq in Generator.GenerateDefault<object>().SeqOf().DistinctBy(function)
                        select (function, seq);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            var (function, seq) = x;
            seq.Map(function).Should().OnlyHaveUniqueItems();
        });
    }

    [FsCheck.NUnit.Property()]
    public Property SubSeqOf_returns_subset_of_items()
    {
        var generator = from items in Generator.GenerateDefault<object>().SeqOf()
                        from subSeq in GenExtensions.SubSeqOf(items)
                        select (items, subSeq);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            var (items, subSeq) = x;
            subSeq.Should().BeSubsetOf(items);
        });
    }

    [Test]
    public void NonEmptySubSeqOf_throws_if_the_source_is_empty()
    {
        var items = Enumerable.Empty<object>();
        var action = () => GenExtensions.NonEmptySubSeqOf(items);
        action.Should().Throw<ArgumentException>();
    }

    [FsCheck.NUnit.Property()]
    public Property NonEmptySubSeqOf_returns_subset_of_items()
    {
        var generator = from items in Generator.GenerateDefault<object>().NonEmptySeqOf()
                        from subSeq in GenExtensions.NonEmptySubSeqOf(items)
                        select (items, subSeq);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            var (items, subSeq) = x;
            subSeq.Should().NotBeEmpty();
            subSeq.Should().BeSubsetOf(items);
        });
    }

    [Test]
    public void OptionOf_generates_right_ratio_of_somes_and_nones()
    {
        var items = Generator.GenerateDefault<object>()
                             .OptionOf()
                             .ListOf(10000)
                             .Sample();

        int somes = 0;
        items.Iter(item =>
        {
            if (item.IsSome)
            {
                Interlocked.Increment(ref somes);
            }
        });

        var somesPercentage = somes / (float)items.Count;
        somesPercentage.Should().BeApproximately(0.875f, 0.1f);
    }
}