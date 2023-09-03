using LanguageExt;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace GuyTheTechie.FunctionalExtensions;

public static class JsonArrayExtensions
{
    public static JsonArray ToJsonArray(this IEnumerable<JsonNode?> nodes) =>
        nodes.Aggregate(new JsonArray(),
                       (array, node) =>
                       {
                           array.Add(node);
                           return array;
                       });

    public static ValueTask<JsonArray> ToJsonArray(this IAsyncEnumerable<JsonNode?> nodes, CancellationToken cancellationToken) =>
        nodes.AggregateAsync(new JsonArray(),
                            (array, node) =>
                            {
                                array.Add(node);
                                return array;
                            },
                            cancellationToken);

    public static Seq<JsonObject> GetJsonObjects(this JsonArray jsonArray) =>
        jsonArray.Choose(node => node.TryAsJsonObject())
                 .ToSeq();

    public static Seq<JsonArray> GetJsonArrays(this JsonArray jsonArray) =>
        jsonArray.Choose(node => node.TryAsJsonArray())
                 .ToSeq();
}
