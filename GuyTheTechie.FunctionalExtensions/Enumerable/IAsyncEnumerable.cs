using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuyTheTechie.FunctionalExtensions;

public static class IAsyncEnumerableExtensions
{
    /// <summary>
    /// Perform <paramref name="action"/> when iterating over each item
    /// </summary>
    public static IAsyncEnumerable<T> Do<T>(this IAsyncEnumerable<T> enumerable, Action<T> action) =>
        enumerable.Select(t =>
        {
            action(t);
            return t;
        });

    public static async ValueTask Iter<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask> action, CancellationToken cancellationToken) =>
        await enumerable.IterParallel(action, maxDegreeOfParallelism: 1, cancellationToken)
                        .ConfigureAwait(false);

    public static async ValueTask IterParallel<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask> action, CancellationToken cancellationToken) =>
        await enumerable.IterParallel(action, maxDegreeOfParallelism: -1, cancellationToken)
                        .ConfigureAwait(false);

    public static async ValueTask IterParallel<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask> action, int maxDegreeOfParallelism, CancellationToken cancellationToken)
    {
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(enumerable, options, async (t, _) => await action(t).ConfigureAwait(false)).ConfigureAwait(false);
    }

    public static async ValueTask<Seq<T>> ToSeq<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken)
    {
        var list = await enumerable.ToListAsync(cancellationToken).ConfigureAwait(false);
        return list.ToSeq();
    }

    public static IAsyncEnumerable<T2> Choose<T, T2>(this IAsyncEnumerable<T> enumerable, Func<T, Option<T2>> f) =>
        enumerable.Select(f)
                  .Where(option => option.IsSome)
                  .Select(option => option.ValueUnsafe());

    public static IAsyncEnumerable<T2> Choose<T, T2>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<Option<T2>>> f) =>
        enumerable.SelectAwait(f)
                  .Where(option => option.IsSome)
                  .Select(option => option.ValueUnsafe());

    public static async ValueTask<Option<T>> HeadOrNone<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken) =>
        await enumerable.Select(Option<T>.Some)
                        .DefaultIfEmpty(Option<T>.None)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
}
