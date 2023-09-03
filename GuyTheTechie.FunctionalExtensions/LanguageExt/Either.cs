using LanguageExt;
using System;

namespace GuyTheTechie.FunctionalExtensions;

public static class EitherExtensions
{
    public static T IfLeftThrow<T>(this Either<string, T> either) =>
        either.IfLeft(left => throw new InvalidOperationException(left));
}