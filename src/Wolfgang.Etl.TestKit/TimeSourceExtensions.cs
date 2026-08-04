using System;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// Extension methods that attach a <see cref="ManualTimeSource"/> to a stage so its
/// <see cref="Report"/> timing metrics can be driven deterministically in tests.
/// </summary>
/// <remarks>
/// These reach the internal time-source seam on the base classes via the friend relationship between
/// <c>Wolfgang.Etl.TestKit</c> and <c>Wolfgang.Etl.Abstractions</c>. Attach the clock before the run
/// starts; the stage captures its start timestamp when the run begins.
/// </remarks>
public static class TimeSourceExtensions
{
    /// <summary>Attaches <paramref name="timeSource"/> to <paramref name="extractor"/>.</summary>
    /// <returns><paramref name="extractor"/>, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either argument is <see langword="null"/>.</exception>
    public static ExtractorBase<TSource, TProgress> WithTimeSource<TSource, TProgress>
    (
        this ExtractorBase<TSource, TProgress> extractor,
        ManualTimeSource timeSource
    )
        where TSource : notnull
        where TProgress : notnull
    {
        if (extractor is null)
        {
            throw new ArgumentNullException(nameof(extractor));
        }

        if (timeSource is null)
        {
            throw new ArgumentNullException(nameof(timeSource));
        }

        extractor.TimeSource = timeSource;
        return extractor;
    }



    /// <summary>Attaches <paramref name="timeSource"/> to <paramref name="loader"/>.</summary>
    /// <returns><paramref name="loader"/>, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either argument is <see langword="null"/>.</exception>
    public static LoaderBase<TDestination, TProgress> WithTimeSource<TDestination, TProgress>
    (
        this LoaderBase<TDestination, TProgress> loader,
        ManualTimeSource timeSource
    )
        where TDestination : notnull
        where TProgress : notnull
    {
        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        if (timeSource is null)
        {
            throw new ArgumentNullException(nameof(timeSource));
        }

        loader.TimeSource = timeSource;
        return loader;
    }



    /// <summary>Attaches <paramref name="timeSource"/> to <paramref name="transformer"/>.</summary>
    /// <returns><paramref name="transformer"/>, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either argument is <see langword="null"/>.</exception>
    public static TransformerBase<TSource, TDestination, TProgress> WithTimeSource<TSource, TDestination, TProgress>
    (
        this TransformerBase<TSource, TDestination, TProgress> transformer,
        ManualTimeSource timeSource
    )
        where TSource : notnull
        where TDestination : notnull
        where TProgress : notnull
    {
        if (transformer is null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        if (timeSource is null)
        {
            throw new ArgumentNullException(nameof(timeSource));
        }

        transformer.TimeSource = timeSource;
        return transformer;
    }
}
