using LanguageExt;
using System;
using System.Text.Json.Nodes;

namespace GuyTheTechie.FunctionalExtensions;

public static class JsonNodeExtensions
{
    public static Option<JsonObject> TryAsJsonObject(this JsonNode? node) =>
        node is JsonObject jsonObject
            ? jsonObject
            : Option<JsonObject>.None;

    public static Option<JsonArray> TryAsJsonArray(this JsonNode? node) =>
        node is JsonArray jsonArray
            ? jsonArray
            : Option<JsonArray>.None;

    public static Option<JsonValue> TryAsJsonValue(this JsonNode? node) =>
        node is JsonValue jsonValue
            ? jsonValue
            : Option<JsonValue>.None;

    public static Option<string> TryAsString(this JsonNode? node) =>
        node.TryAsJsonValue()
            .Bind(JsonValueExtensions.TryAsString);

    public static Option<Guid> TryAsGuid(this JsonNode? node) =>
        node.TryAsJsonValue()
            .Bind(JsonValueExtensions.TryAsGuid);

    public static Option<Uri> TryAsAbsoluteUri(this JsonNode? node) =>
        node.TryAsJsonValue()
            .Bind(JsonValueExtensions.TryAsAbsoluteUri);

    public static Option<DateTimeOffset> TryAsDateTimeOffset(this JsonNode? node) =>
        node.TryAsJsonValue()
            .Bind(JsonValueExtensions.TryAsDateTimeOffset);

    public static Option<DateTime> TryAsDateTime(this JsonNode? node) =>
        node.TryAsJsonValue()
            .Bind(JsonValueExtensions.TryAsDateTime);

    public static Option<int> TryAsInt(this JsonNode? node) =>
        node.TryAsJsonValue()
            .Bind(JsonValueExtensions.TryAsInt);

    public static Option<double> TryAsDouble(this JsonNode? node) =>
        node.TryAsJsonValue()
            .Bind(JsonValueExtensions.TryAsDouble);

    public static Option<bool> TryAsBool(this JsonNode? node) =>
        node.TryAsJsonValue()
            .Bind(JsonValueExtensions.TryAsBool);
}
