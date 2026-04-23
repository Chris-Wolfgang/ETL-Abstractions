using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// An <see cref="IExtractStage{TSource}"/> whose underlying extractor also supports progress
/// reporting. Call <see cref="WithProgress"/> to bind an <see cref="IProgress{TProgress}"/>
/// sink before proceeding to <c>Transform</c> or <c>Load</c>.
/// </summary>
/// <typeparam name="TSource">The type of item produced by the extractor.</typeparam>
/// <typeparam name="TProgress">The type of progress report emitted by the extractor.</typeparam>
public interface IExtractStageWithProgress<TSource, TProgress> : IExtractStage<TSource>
    where TSource : notnull
    where TProgress : notnull
{
    /// <summary>
    /// Binds an <see cref="IProgress{TProgress}"/> sink to the extractor. The returned stage
    /// is a plain <see cref="IExtractStage{TSource}"/> — progress can only be set once.
    /// </summary>
    /// <param name="progress">The progress sink to forward reports to.</param>
    /// <returns>The same stage, with progress bound, typed as <see cref="IExtractStage{TSource}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="progress"/> is <see langword="null"/>.</exception>
    IExtractStage<TSource> WithProgress(IProgress<TProgress> progress);
}
