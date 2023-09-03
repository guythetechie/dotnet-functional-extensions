using FluentAssertions;
using FluentAssertions.LanguageExt;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;

namespace GuyTheTechie.FunctionalExtensions.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class JsonArrayTests
{
    [FsCheck.NUnit.Property()]
    public Property ToJsonArray_converts_the_enumerable_of_nodes_to_a_JSON_array()
    {
        var generator = Generator.JsonNode.SeqOf();
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, list =>
        {
            // Act
            var jsonArray = list.ToJsonArray();

            // Assert
            jsonArray.Should().BeEquivalentTo(list);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property ToJsonArray_converts_the_asyncenumerable_of_nodes_to_a_JSON_array()
    {
        var generator = Generator.JsonNode.SeqOf();
        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, async list =>
        {
            // Arrange
            var asyncEnumerable = list.ToAsyncEnumerable();

            // Act
            var jsonArray = await asyncEnumerable.ToJsonArray(CancellationToken.None);

            // Assert
            jsonArray.Should().BeEquivalentTo(list);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetJsonObjects_returns_JSON_objects_in_JSON_array()
    {
        var generator = from jsonObjects in GenerateJsonObjectNodes()
                        from nonJsonObjects in from jsonArrays in GenerateJsonArrayNodes()
                                               from jsonValues in GenerateJsonValueNodes()
                                               select jsonArrays.Concat(jsonValues)
                        let jsonArray = new JsonArray(jsonObjects.Concat(nonJsonObjects).ToArray())
                        select (jsonArray, jsonObjects);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonArray, expectedJsonObjects) = x;

            // Act
            var jsonObjects = jsonArray.GetJsonObjects();

            // Assert
            jsonObjects.Should().BeEquivalentTo(expectedJsonObjects);
        });
    }

    [FsCheck.NUnit.Property()]
    public Property GetJsonArrays_returns_JSON_arrays_in_JSON_array()
    {
        var generator = from jsonArrays in GenerateJsonArrayNodes()
                        from nonJsonArrays in from jsonObjects in GenerateJsonObjectNodes()
                                              from jsonValues in GenerateJsonValueNodes()
                                              select jsonObjects.Concat(jsonValues)
                        let jsonArray = new JsonArray(jsonArrays.Concat(nonJsonArrays).ToArray())
                        select (jsonArray, jsonArrays);

        var arbitrary = generator.ToArbitrary();

        return Prop.ForAll(arbitrary, x =>
        {
            // Arrange
            var (jsonArray, expectedJsonArrays) = x;

            // Act
            var jsonObjects = jsonArray.GetJsonArrays();

            // Assert
            jsonObjects.Should().BeEquivalentTo(expectedJsonArrays);
        });
    }

    private static Gen<Seq<JsonNode?>> GenerateJsonObjectNodes() =>
        Generator.JsonObject.Select(x => (JsonNode?)x)
                 .SeqOf();

    private static Gen<Seq<JsonNode?>> GenerateJsonArrayNodes() =>
        Generator.JsonArray.Select(x => (JsonNode?)x)
                 .SeqOf();

    private static Gen<Seq<JsonNode?>> GenerateJsonValueNodes() =>
        Generator.JsonValue.Select(x => (JsonNode?)x)
                 .SeqOf();
}