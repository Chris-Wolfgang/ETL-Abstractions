using System;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// Extension methods that attach a <see cref="ManualProgressTimerCore"/> to a stage so its progress
/// callback fires deterministically (on <see cref="ManualProgressTimerCore.Tick"/>) instead of on a
/// real timer interval.
/// </summary>
/// <remarks>
/// These reach the internal timer-core seam on the base classes via the friend relationship between
/// <c>Wolfgang.Etl.TestKit</c> and <c>Wolfgang.Etl.Abstractions</c>, so a component needs no
/// per-type timer-injection plumbing. Attach the timer before the run starts; the stage builds its
/// progress timer when the run begins.
/// </remarks>
public static class ProgressTimerExtensions
{
    /// <summary>Attaches <paramref name="timer"/> to <paramref name="extractor"/>.</summary>
    /// <returns><paramref name="extractor"/>, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either argument is <see langword="null"/>.</exception>
    public static ExtractorBase<TSource, TProgress> WithManualProgressTimer<TSource, TProgress>
    (
        this ExtractorBase<TSource, TProgress> extractor,
        ManualProgressTimerCore timer
    )
        where TSource : notnull
        where TProgress : notnull
    {
        if (extractor is null)
        {
            throw new ArgumentNullException(nameof(extractor));
        }

        if (timer is null)
        {
            throw new ArgumentNullException(nameof(timer));
        }

        extractor.TimerCoreFactory = timer.CoreFactory;
        return extractor;
    }



    /// <summary>Attaches <paramref name="timer"/> to <paramref name="loader"/>.</summary>
    /// <returns><paramref name="loader"/>, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either argument is <see langword="null"/>.</exception>
    public static LoaderBase<TDestination, TProgress> WithManualProgressTimer<TDestination, TProgress>
    (
        this LoaderBase<TDestination, TProgress> loader,
        ManualProgressTimerCore timer
    )
        where TDestination : notnull
        where TProgress : notnull
    {
        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        if (timer is null)
        {
            throw new ArgumentNullException(nameof(timer));
        }

        loader.TimerCoreFactory = timer.CoreFactory;
        return loader;
    }



    /// <summary>Attaches <paramref name="timer"/> to <paramref name="transformer"/>.</summary>
    /// <returns><paramref name="transformer"/>, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either argument is <see langword="null"/>.</exception>
    public static TransformerBase<TSource, TDestination, TProgress> WithManualProgressTimer<TSource, TDestination, TProgress>
    (
        this TransformerBase<TSource, TDestination, TProgress> transformer,
        ManualProgressTimerCore timer
    )
        where TSource : notnull
        where TDestination : notnull
        where TProgress : notnull
    {
        if (transformer is null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        if (timer is null)
        {
            throw new ArgumentNullException(nameof(timer));
        }

        transformer.TimerCoreFactory = timer.CoreFactory;
        return transformer;
    }
}
