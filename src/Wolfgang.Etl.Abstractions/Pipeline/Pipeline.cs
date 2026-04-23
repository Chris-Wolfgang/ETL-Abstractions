using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Entry point for building a fluent ETL pipeline. Start with <see cref="Extract{TSource}"/>,
/// chain zero or more <c>Transform</c> calls, terminate with <c>Load</c>, then invoke
/// <see cref="IPipeline.RunAsync()"/>.
/// </summary>
/// <example>
/// <code>
/// await Pipeline
///     .Extract(csvExtractor).WithProgress(extractProgress)
///     .Transform(enrich)
///     .Load(sqlLoader).WithProgress(loadProgress)
///     .WithName("nightly-import")
///     .RunAsync(token);
/// </code>
/// </example>
public static class Pipeline
{
    /// <summary>
    /// Begins a pipeline from an extractor that does not report progress.
    /// </summary>
    /// <typeparam name="TSource">The type of item produced by the extractor.</typeparam>
    /// <param name="extractor">The extractor that seeds the pipeline.</param>
    /// <returns>An <see cref="IExtractStage{TSource}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="extractor"/> is <see langword="null"/>.</exception>
    public static IExtractStage<TSource> Extract<TSource>
    (
        IExtractWithCancellationAsync<TSource> extractor
    )
        where TSource : notnull
    {
        if (extractor is null)
        {
            throw new ArgumentNullException(nameof(extractor));
        }

        return new ExtractStage<TSource>(token => extractor.ExtractAsync(token));
    }


    /// <summary>
    /// Begins a pipeline from a progress-reporting extractor. The returned stage exposes
    /// <see cref="IExtractStageWithProgress{TSource, TProgress}.WithProgress"/> so a progress sink
    /// can be bound before appending further stages.
    /// </summary>
    /// <typeparam name="TSource">The type of item produced by the extractor.</typeparam>
    /// <typeparam name="TProgress">The type of progress report emitted by the extractor.</typeparam>
    /// <param name="extractor">The extractor that seeds the pipeline.</param>
    /// <returns>An <see cref="IExtractStageWithProgress{TSource, TProgress}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="extractor"/> is <see langword="null"/>.</exception>
    public static IExtractStageWithProgress<TSource, TProgress> Extract<TSource, TProgress>
    (
        IExtractWithProgressAndCancellationAsync<TSource, TProgress> extractor
    )
        where TSource : notnull
        where TProgress : notnull
    {
        if (extractor is null)
        {
            throw new ArgumentNullException(nameof(extractor));
        }

        return new ExtractStageWithProgress<TSource, TProgress>(extractor);
    }
}
