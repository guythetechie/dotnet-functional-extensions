using LanguageExt;
using System;
using System.Text.Json.Nodes;

namespace GuyTheTechie.FunctionalExtensions;

public static class JsonValueExtensions
{
    public static Option<string> TryAsString(this JsonValue? jsonValue) =>
        jsonValue is not null && jsonValue.TryGetValue<string>(out var value)
            ? value
            : Option<string>.None;

    public static Option<Guid> TryAsGuid(this JsonValue? jsonValue) =>
        jsonValue is not null && jsonValue.TryGetValue<Guid>(out var guid)
            ? guid
            : jsonValue.TryAsString()
                       .Bind(x => Guid.TryParse(x, out var guidString)
                                    ? guidString
                                    : Option<Guid>.None);

    public static Option<Uri> TryAsAbsoluteUri(this JsonValue? jsonValue) =>
        jsonValue.TryAsString()
                 .Bind(x => Uri.TryCreate(x, UriKind.Absolute, out var uri)
                                ? uri
                                : Option<Uri>.None);

    public static Option<DateTimeOffset> TryAsDateTimeOffset(this JsonValue? jsonValue) =>
        jsonValue is not null && jsonValue.TryGetValue<DateTimeOffset>(out var dateTimeOffset)
            ? dateTimeOffset
            : jsonValue.TryAsString()
                       .Bind(x => DateTimeOffset.TryParse(x, out var dateTime)
                                    ? dateTime
                                    : Option<DateTimeOffset>.None);

    public static Option<DateTime> TryAsDateTime(this JsonValue? jsonValue) =>
        jsonValue is not null && jsonValue.TryGetValue<DateTime>(out var dateTime)
            ? dateTime
            : jsonValue.TryAsString()
                       .Bind(x => DateTime.TryParse(x, out var dateTime)
                                    ? dateTime
                                    : Option<DateTime>.None);

    public static Option<int> TryAsInt(this JsonValue? jsonValue) =>
        jsonValue is not null && jsonValue.TryGetValue<int>(out var value)
            ? value
            : Option<int>.None;

    public static Option<double> TryAsDouble(this JsonValue? jsonValue) =>
    jsonValue is not null && jsonValue.TryGetValue<double>(out var value)
        ? value
        : Option<double>.None;

    public static Option<bool> TryAsBool(this JsonValue? jsonValue) =>
        jsonValue is not null && jsonValue.TryGetValue<bool>(out var value)
            ? value
            : Option<bool>.None;
}
