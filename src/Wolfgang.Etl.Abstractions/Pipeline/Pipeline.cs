using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Entry point for building a fluent ETL pipeline. Start with one of the <c>Extract</c> overloads,
/// chain zero or more <c>Transform</c> calls, terminate with one of the <c>Load</c> overloads, then
/// invoke <see cref="IPipeline.RunAsync()"/>.
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
/// <remarks>
/// <b>Overload resolution and capability interfaces.</b> The <c>Extract</c>, <c>Transform</c>, and
/// <c>Load</c> methods are overloaded by interface capability — no-progress/no-cancellation, with
/// cancellation, with progress, or both. C# overload resolution uses the <i>static</i> type at the
/// call site. A class that implements
/// <see cref="IExtractWithProgressAndCancellationAsync{TSource, TProgress}"/> but is passed via a
/// variable declared as <see cref="IExtractAsync{TSource}"/> will silently bind to the bare
/// overload, and the pipeline will neither forward a cancellation token nor accept a progress
/// sink. Pass stages using their most-derived interface (or concrete class) to get the intended
/// behavior.
/// </remarks>
public static class Pipeline
{
    /// <summary>
    /// Begins a pipeline from an extractor that supports neither progress nor cancellation. The
    /// pipeline's <see cref="System.Threading.CancellationToken"/> will not be forwarded into this
    /// stage.
    /// </summary>
    /// <typeparam name="TSource">The type of item produced by the extractor.</typeparam>
    /// <param name="extractor">The extractor that seeds the pipeline.</param>
    /// <returns>An <see cref="IExtractStage{TSource}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="extractor"/> is <see langword="null"/>.</exception>
    public static IExtractStage<TSource> Extract<TSource>
    (
        IExtractAsync<TSource> extractor
    )
        where TSource : notnull
    {
        if (extractor is null)
        {
            throw new ArgumentNullException(nameof(extractor));
        }

        return new ExtractStage<TSource>(_ => extractor.ExtractAsync());
    }


    /// <summary>
    /// Begins a pipeline from an extractor that supports cancellation but does not report progress.
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
    /// Begins a pipeline from a progress-reporting extractor that does not support cancellation.
    /// The pipeline's <see cref="System.Threading.CancellationToken"/> will not be forwarded into
    /// this stage. The returned stage exposes
    /// <see cref="IExtractStageWithProgress{TSource, TProgress}.WithProgress"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of item produced by the extractor.</typeparam>
    /// <typeparam name="TProgress">The type of progress report emitted by the extractor.</typeparam>
    /// <param name="extractor">The extractor that seeds the pipeline.</param>
    /// <returns>An <see cref="IExtractStageWithProgress{TSource, TProgress}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="extractor"/> is <see langword="null"/>.</exception>
    public static IExtractStageWithProgress<TSource, TProgress> Extract<TSource, TProgress>
    (
        IExtractWithProgressAsync<TSource, TProgress> extractor
    )
        where TSource : notnull
        where TProgress : notnull
    {
        if (extractor is null)
        {
            throw new ArgumentNullException(nameof(extractor));
        }

        return new ExtractStageWithProgress<TSource, TProgress>
        (
            noProgressSource: _ => extractor.ExtractAsync(),
            withProgressSource: (progress, _) => extractor.ExtractAsync(progress)
        );
    }


    /// <summary>
    /// Begins a pipeline from a progress-reporting extractor that also supports cancellation. The
    /// returned stage exposes <see cref="IExtractStageWithProgress{TSource, TProgress}.WithProgress"/>.
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

        return new ExtractStageWithProgress<TSource, TProgress>
        (
            noProgressSource: token => extractor.ExtractAsync(token),
            withProgressSource: (progress, token) => extractor.ExtractAsync(progress, token)
        );
    }
}
