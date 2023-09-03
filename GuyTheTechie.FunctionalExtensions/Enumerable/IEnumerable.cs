using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GuyTheTechie.FunctionalExtensions;

public static class IEnumerableExtensions
{
    public static async ValueTask Iter<T>(this IEnumerable<T> enumerable, Func<T, ValueTask> action, CancellationToken cancellationToken)
    {
        await enumerable.IterParallel(action, maxDegreeOfParallelism: 1, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask IterParallel<T>(this IEnumerable<T> enumerable, Func<T, ValueTask> action, CancellationToken cancellationToken)
    {
        await enumerable.IterParallel(action, maxDegreeOfParallelism: -1, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask IterParallel<T>(this IEnumerable<T> enumerable, Func<T, ValueTask> action, int maxDegreeOfParallelism, CancellationToken cancellationToken)
    {
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(enumerable, options, async (t, _) => await action(t).ConfigureAwait(false)).ConfigureAwait(false);
    }
}
