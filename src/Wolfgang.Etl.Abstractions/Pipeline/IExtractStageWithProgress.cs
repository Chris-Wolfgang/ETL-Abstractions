using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// An <see cref="IExtractStage{TSource}"/> whose underlying extractor also supports progress
/// reporting. Call <see cref="WithProgress"/> to supply an <see cref="IProgress{TProgress}"/>
/// sink that will be forwarded to the extractor's <c>ExtractAsync</c> overload when the
/// pipeline runs.
/// </summary>
/// <typeparam name="TSource">The type of item produced by the extractor.</typeparam>
/// <typeparam name="TProgress">The type of progress report emitted by the extractor.</typeparam>
public interface IExtractStageWithProgress<TSource, out TProgress> : IExtractStage<TSource>
    where TSource : notnull
    where TProgress : notnull
{
    /// <summary>
    /// Supplies an <see cref="IProgress{TProgress}"/> sink that the pipeline will pass to the
    /// extractor's <c>ExtractAsync</c> overload when the pipeline runs. The returned stage is
    /// a plain <see cref="IExtractStage{TSource}"/> — progress can only be set once.
    /// </summary>
    /// <param name="progress">The progress sink to forward reports to.</param>
    /// <returns>The same stage, typed as <see cref="IExtractStage{TSource}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="progress"/> is <see langword="null"/>.</exception>
    IExtractStage<TSource> WithProgress(IProgress<TProgress> progress);
}
