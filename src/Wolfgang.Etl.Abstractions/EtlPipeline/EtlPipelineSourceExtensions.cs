using System;
using System.Collections.Generic;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Built-in source factories for <see cref="EtlPipeline"/>. They begin a pipeline from a raw stream or
/// an extractor. Format packages add their own class-named source factories (for example
/// <c>CsvExtractor</c>) as extension methods on <see cref="EtlPipeline"/> alongside these, so every
/// source reads the same way: <c>EtlPipeline.Create().Xxx(...)</c>.
/// </summary>
public static class EtlPipelineSourceExtensions
{
    /// <summary>
    /// Begins a pipeline from an existing asynchronous stream — the generic escape hatch for any
    /// source the caller already has as an <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item produced by the source.</typeparam>
    /// <param name="pipeline">The builder returned by <see cref="EtlPipeline.Create"/>.</param>
    /// <param name="source">The stream that seeds the pipeline.</param>
    /// <returns>An <see cref="IEtlPipeline{T}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEtlPipeline<T> From<T>(this EtlPipeline pipeline, IAsyncEnumerable<T> source)
        where T : notnull
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return EtlPipelineImpl<T>.FromStream((_, _) => source);
    }


    /// <summary>
    /// Begins a pipeline from an <see cref="ExtractorBase{TSource, TProgress}"/>. The pipeline's
    /// cancellation token is forwarded to the extractor.
    /// </summary>
    /// <typeparam name="T">The type of item produced by the extractor.</typeparam>
    /// <typeparam name="TProgress">The extractor's progress-report type.</typeparam>
    /// <param name="pipeline">The builder returned by <see cref="EtlPipeline.Create"/>.</param>
    /// <param name="extractor">The extractor that seeds the pipeline.</param>
    /// <returns>An <see cref="IEtlPipeline{T}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="extractor"/> is <see langword="null"/>.</exception>
    public static IEtlPipeline<T> From<T, TProgress>(this EtlPipeline pipeline, ExtractorBase<T, TProgress> extractor)
        where T : notnull
        where TProgress : notnull
    {
        if (extractor is null)
        {
            throw new ArgumentNullException(nameof(extractor));
        }

        return EtlPipelineImpl<T>.FromStream((state, token) =>
        {
            // Surface the extractor's skipped-item count into the pipeline snapshot so a bad record
            // the extractor's error policy discarded is reported, not silently absent from the totals.
            state.SkippedCountReader = () => extractor.CurrentSkippedItemCount;
            return extractor.ExtractAsync(token);
        });
    }
}
