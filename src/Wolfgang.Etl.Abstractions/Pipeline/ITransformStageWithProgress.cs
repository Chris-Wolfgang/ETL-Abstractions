using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// An <see cref="ITransformStage{TSource}"/> whose underlying transformer also supports progress
/// reporting. Call <see cref="WithProgress"/> to supply an <see cref="IProgress{TProgress}"/>
/// sink that will be forwarded to the transformer's <c>TransformAsync</c> overload when the
/// pipeline runs.
/// </summary>
/// <typeparam name="TSource">The current item type flowing through the pipeline.</typeparam>
/// <typeparam name="TProgress">The type of progress report emitted by the transformer.</typeparam>
public interface ITransformStageWithProgress<TSource, TProgress> : ITransformStage<TSource>
    where TSource : notnull
    where TProgress : notnull
{
    /// <summary>
    /// Supplies an <see cref="IProgress{TProgress}"/> sink that the pipeline will pass to the
    /// transformer's <c>TransformAsync</c> overload when the pipeline runs. The returned stage
    /// is a plain <see cref="ITransformStage{TSource}"/> — progress can only be set once.
    /// </summary>
    /// <param name="progress">The progress sink to forward reports to.</param>
    /// <returns>The same stage, typed as <see cref="ITransformStage{TSource}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="progress"/> is <see langword="null"/>.</exception>
    ITransformStage<TSource> WithProgress(IProgress<TProgress> progress);
}
