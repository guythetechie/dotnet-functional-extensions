using Bogus.DataSets;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace GuyTheTechie.FunctionalExtensions;

public static class Generator
{
    public static Gen<int> PositiveInteger { get; } =
        from positiveInt in GenerateDefault<PositiveInt>()
        select positiveInt.Get;

    public static Gen<int> NegativeInteger { get; } =
        from negativeInt in GenerateDefault<NegativeInt>()
        select negativeInt.Get;

    private static char[] AlphaNumericChars { get; } =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    public static Gen<string> AlphaNumericString { get; } =
        Gen.Elements(AlphaNumericChars)
           .NonEmptyListOf()
           .Select(x => new string(x.ToArray()));

    public static Gen<string> GuidString { get; } =
        from guid in GenerateDefault<Guid>()
        select guid.ToString();

    public static Gen<string> EmptyOrWhiteSpaceString { get; } =
        Gen.OneOf(Gen.Constant(string.Empty),
                  GenerateDefault<char>()
                               .Where(char.IsWhiteSpace)
                               .ArrayOf()
                               .Select(string.Concat));

    public static Gen<string> NonEmptyOrWhiteSpaceString { get; } =
        GenerateDefault<NonWhiteSpaceString>()
            .Select(x => x.Item)
            .Where(x => string.IsNullOrWhiteSpace(x) is false);

    public static Gen<Internet> Internet { get; } =
        Gen.Constant(new Internet());

    public static Gen<Uri> AbsoluteUri { get; } =
        from internet in Internet
        let url = internet.Url()
        select new Uri(url);

    public static Gen<JsonNode> JsonNode { get; } = GenerateJsonNode();

    public static Gen<JsonObject> JsonObject { get; } = GenerateJsonObject();

    public static Gen<JsonValue> JsonValue { get; } = GenerateJsonValue();

    public static Gen<JsonArray> JsonArray { get; } = GenerateJsonArray();

    public static Gen<T> GenerateDefault<T>() =>
        ArbMap.Default.GeneratorFor<T>();

    private static Gen<JsonValue> GenerateJsonValue()
    {
        return Gen.OneOf(GenerateJsonValue<bool>(),
                         GenerateJsonValue<byte>(),
                         GenerateJsonValue<char>(),
                         GenerateJsonValue<DateTime>(),
                         GenerateJsonValue<DateTimeOffset>(),
                         GenerateJsonValue<decimal>(),
                         GenerateJsonValue<double>(),
                         GenerateJsonValue<Guid>(),
                         GenerateJsonValue<short>(),
                         GenerateJsonValue<int>(),
                         GenerateJsonValue<long>(),
                         GenerateJsonValue<sbyte>(),
                         GenerateJsonValue<float>(),
                         GenerateJsonValue<string>(),
                         GenerateJsonValue<ushort>(),
                         GenerateJsonValue<uint>(),
                         GenerateJsonValue<ulong>());
    }

    public static Gen<JsonValue> GenerateJsonValue<T>()
    {
        var generator = typeof(T) switch
        {
            var type when type == typeof(double) => GenerateDefault<double>()
                                                        .Where(double.IsFinite)
                                                        .Where(d => double.IsNaN(d) is false)
                                                        .Select(d => (T)(object)d),
            var type when type == typeof(float) => GenerateDefault<float>()
                                                        .Where(float.IsFinite)
                                                        .Where(f => float.IsNaN(f) is false)
                                                        .Select(f => (T)(object)f),
            var type when type == typeof(string) => Gen.OneOf(AbsoluteUri.Select(x => x.ToString()),
                                                              GenerateDefault<string>())
                                                       .Select(s => (T)(object)s),
            _ => GenerateDefault<T>()
        };

        return from t in generator
               from jsonValue in t is DateTimeOffset or DateTime or Guid
                                    ? Gen.Elements(System.Text.Json.Nodes.JsonValue.Create(t.ToString()),
                                                   System.Text.Json.Nodes.JsonValue.Create(t))
                                    : Gen.Constant(System.Text.Json.Nodes.JsonValue.Create(t))
               select jsonValue;
    }

    private static Gen<JsonNode> GenerateJsonNode() =>
        Gen.Sized(GenerateJsonNode);

    private static Gen<JsonNode> GenerateJsonNode(int size) =>
        size < 1
        ? GenerateJsonValue().Select(value => value as JsonNode)
        : Gen.OneOf(from jsonValue in GenerateJsonValue()
                    select jsonValue as JsonNode,
                    from jsonObject in GenerateJsonObject(size / 2)
                    select jsonObject as JsonNode,
                    from jsonArray in GenerateJsonArray(size / 2)
                    select jsonArray as JsonNode);

    private static Gen<JsonObject> GenerateJsonObject() =>
        Gen.Sized(GenerateJsonObject);

    private static Gen<JsonObject> GenerateJsonObject(int size) =>
        Gen.Zip(AlphaNumericString, GenerateJsonNode(size).OrNull())
           .Select(x => KeyValuePair.Create(x.Item1, (JsonNode?)x.Item2))
           .ListOf()
           .Select(list => list.DistinctBy(x => x.Key.ToUpperInvariant()))
           .Select(items => new JsonObject(items));

    private static Gen<JsonArray> GenerateJsonArray() =>
        Gen.Sized(GenerateJsonArray);

    private static Gen<JsonArray> GenerateJsonArray(int size) =>
        GenerateJsonNode(size)
            .OrNull()
            .ArrayOf()
            .Select(array => new JsonArray(array));
}

public static class GenExtensions
{
    public static Gen<Seq<T>> SeqOf<T>(this Gen<T> gen) =>
        gen.ListOf()
           .Select(x => x.ToSeq());

    public static Gen<Seq<T>> SeqOf<T>(this Gen<T> gen, uint minimum, uint maximum)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minimum, maximum);

        return from count in Gen.Choose((int)minimum, (int)maximum)
               from list in gen.ListOf(count)
               select list.ToSeq();
    }

    public static Gen<Seq<T>> SubSeqOf<T>(IEnumerable<T> items) =>
        Gen.SubListOf(items)
           .Select(x => x.ToSeq());

    public static Gen<Seq<T>> NonEmptySubSeqOf<T>(IEnumerable<T> items) =>
        items.Any()
        ? SubSeqOf(items).Where(items => items.IsEmpty is false)
        : throw new ArgumentException("The collection must not be empty.", nameof(items));

    public static Gen<Seq<T>> NonEmptySeqOf<T>(this Gen<T> gen) =>
        gen.NonEmptyListOf()
           .Select(x => x.ToSeq());

    public static Gen<Seq<T>> DistinctBy<T, TKey>(this Gen<Seq<T>> gen, Func<T, TKey> keySelector) =>
        gen.Select(x => x.DistinctBy(keySelector)
                         .ToSeq());

    public static Gen<Option<T>> OptionOf<T>(this Gen<T> gen) =>
        Gen.Frequency((1, Gen.Constant(Option<T>.None)),
                      (7, gen.Select(Option<T>.Some)));

    public static T Sample<T>(this Gen<T> gen) =>
        gen.Sample(1).First();
}