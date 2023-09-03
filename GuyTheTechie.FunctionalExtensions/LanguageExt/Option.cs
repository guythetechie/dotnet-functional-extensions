using LanguageExt;
using System;

namespace GuyTheTechie.FunctionalExtensions;

public static class OptionExtensions
{
    public static T IfNoneThrow<T>(this Option<T> option, string message) =>
        option.IfNone(() => throw new InvalidOperationException(message));
}