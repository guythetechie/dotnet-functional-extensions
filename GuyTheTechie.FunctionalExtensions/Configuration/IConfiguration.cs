using System;
using LanguageExt;
using Microsoft.Extensions.Configuration;

namespace GuyTheTechie.FunctionalExtensions;

public static class ConfigurationExtensions
{
    public static string GetValue(this IConfiguration configuration, string key) =>
        configuration.TryGetValue(key)
                     .IfNoneThrow($"Could not find '{key}' in configuration.");

    public static Option<string> TryGetValue(this IConfiguration configuration, string key) =>
        configuration.TryGetSection(key)
                     .Where(section => section.Value is not null)
                     .Select(section => section.Value!);

    public static Option<IConfigurationSection> TryGetSection(this IConfiguration configuration, string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(key);

        return section.Exists()
                ? Option<IConfigurationSection>.Some(section)
                : Option<IConfigurationSection>.None;
    }
}