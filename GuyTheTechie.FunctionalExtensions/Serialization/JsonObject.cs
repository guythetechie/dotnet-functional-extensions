using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GuyTheTechie.FunctionalExtensions;

public static class JsonObjectExtensions
{
    public static JsonNode GetProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Option<JsonNode> GetOptionalProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .ToOption();

    public static Either<string, JsonNode> TryGetProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject is null
            ? "JSON object is null."
            : jsonObject.TryGetPropertyValue(propertyName, out var node)
                ? node is null
                    ? $"Property '{propertyName}' is null."
                    : Either<string, JsonNode>.Right(node)
                : $"Property '{propertyName}' is missing.";

    public static JsonObject GetJsonObjectProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetJsonObjectProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, JsonObject> TryGetJsonObjectProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsJsonObject(propertyName));

    private static Either<string, JsonObject> TryAsJsonObject(this JsonNode node, string propertyName) =>
        node.TryAsJsonObject()
            .ToEither(() => $"Property '{propertyName}' is not a JSON object.");

    public static JsonArray GetJsonArrayProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetJsonArrayProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, JsonArray> TryGetJsonArrayProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsJsonArray(propertyName));

    private static Either<string, JsonArray> TryAsJsonArray(this JsonNode node, string propertyName) =>
        node.TryAsJsonArray()
            .ToEither(() => $"Property '{propertyName}' is not a JSON array.");

    public static JsonValue GetJsonValueProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetJsonValueProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, JsonValue> TryGetJsonValueProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsJsonValue(propertyName));

    private static Either<string, JsonValue> TryAsJsonValue(this JsonNode node, string propertyName) =>
        node.TryAsJsonValue()
            .ToEither(() => $"Property '{propertyName}' is not a JSON value.");

    public static string GetStringProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetStringProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, string> TryGetStringProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsString(propertyName));

    private static Either<string, string> TryAsString(this JsonNode node, string propertyName) =>
        node.TryAsString()
            .ToEither(() => $"Property '{propertyName}' is not a string.");

    public static string GetNonEmptyOrWhiteSpaceStringProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetNonEmptyOrWhiteSpaceStringProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, string> TryGetNonEmptyOrWhiteSpaceStringProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetStringProperty(propertyName)
                  .Bind(value => string.IsNullOrWhiteSpace(value)
                                 ? Either<string, string>.Left($"Property '{propertyName}' is empty or whitespace.")
                                 : Either<string, string>.Right(value));

    public static Guid GetGuidProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetGuidProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, Guid> TryGetGuidProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsGuid(propertyName));

    private static Either<string, Guid> TryAsGuid(this JsonNode node, string propertyName) =>
        node.TryAsGuid()
            .ToEither(() => $"Property '{propertyName}' is not a GUID.");

    public static Uri GetAbsoluteUriProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetAbsoluteUriProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, Uri> TryGetAbsoluteUriProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsAbsoluteUri(propertyName));

    public static DateTimeOffset GetDateTimeOffsetProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetDateTimeOffsetProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, DateTimeOffset> TryGetDateTimeOffsetProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsDateTimeOffset(propertyName));

    private static Either<string, DateTimeOffset> TryAsDateTimeOffset(this JsonNode node, string propertyName) =>
        node.TryAsDateTimeOffset()
            .ToEither(() => $"Property '{propertyName}' is not a valid DateTimeOffset.");

    public static DateTime GetDateTimeProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetDateTimeProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, DateTime> TryGetDateTimeProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsDateTime(propertyName));

    private static Either<string, DateTime> TryAsDateTime(this JsonNode node, string propertyName) =>
        node.TryAsDateTime()
            .ToEither(() => $"Property '{propertyName}' is not a valid DateTime.");

    private static Either<string, Uri> TryAsAbsoluteUri(this JsonNode node, string propertyName) =>
        node.TryAsAbsoluteUri()
            .ToEither(() => $"Property '{propertyName}' is not an absolute URI.");

    public static int GetIntProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetIntProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, int> TryGetIntProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsInt(propertyName));

    private static Either<string, int> TryAsInt(this JsonNode node, string propertyName) =>
        node.TryAsInt()
            .ToEither(() => $"Property '{propertyName}' is not an integer.");

    public static double GetDoubleProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetDoubleProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, double> TryGetDoubleProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsDouble(propertyName));

    private static Either<string, double> TryAsDouble(this JsonNode node, string propertyName) =>
        node.TryAsDouble()
            .ToEither(() => $"Property '{propertyName}' is not a double.");

    public static bool GetBoolProperty(this JsonObject jsonObject, string propertyName) =>
        jsonObject.TryGetBoolProperty(propertyName)
                  .IfLeftThrowJsonException();

    public static Either<string, bool> TryGetBoolProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.TryGetProperty(propertyName)
                  .Bind(node => node.TryAsBool(propertyName));

    private static Either<string, bool> TryAsBool(this JsonNode node, string propertyName) =>
        node.TryAsBool()
            .ToEither(() => $"Property '{propertyName}' is not a boolean.");

    private static T IfLeftThrowJsonException<T>(this Either<string, T> either)
    {
        return either.IfLeft(left => throw new JsonException(left));
    }

    [return: NotNullIfNotNull(nameof(jsonObject))]
    public static JsonObject? SetProperty(this JsonObject? jsonObject, string propertyName, JsonNode? jsonNode)
    {
        if (jsonObject is null)
        {
            return null;
        }
        else
        {
            jsonObject[propertyName] = jsonNode;
            return jsonObject;
        }
    }

    /// <summary>
    /// Sets <paramref name="jsonObject"/>[<paramref name="propertyName"/>] = <paramref name="jsonNode"/> if <paramref name="jsonNode"/> is not null.
    /// </summary>
    [return: NotNullIfNotNull(nameof(jsonObject))]
    public static JsonObject? SetPropertyIfNotNull(this JsonObject? jsonObject, string propertyName, JsonNode? jsonNode) =>
        jsonNode is null
            ? jsonObject
            : jsonObject.SetProperty(propertyName, jsonNode);

    /// <summary>
    /// Sets <paramref name="jsonObject"/>'s property <paramref name="propertyName"/> to the value of <paramref name="option"/> if <paramref name="option"/> is Some.
    /// </summary>
    [return: NotNullIfNotNull(nameof(jsonObject))]
    public static JsonObject? SetPropertyIfSome(this JsonObject? jsonObject, string propertyName, Option<JsonNode> option) =>
        jsonObject.SetPropertyIfNotNull(propertyName, option.ValueUnsafe());

    [return: NotNullIfNotNull(nameof(jsonObject))]
    public static JsonObject? RemoveProperty(this JsonObject? jsonObject, string propertyName)
    {
        if (jsonObject is null)
        {
            return null;
        }
        else
        {
            jsonObject.Remove(propertyName);
            return jsonObject;
        }
    }
}