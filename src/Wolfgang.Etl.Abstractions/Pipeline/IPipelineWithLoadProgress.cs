using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// An <see cref="IPipeline"/> whose loader supports progress reporting. Call
/// <see cref="WithProgress"/> to bind an <see cref="IProgress{TProgress}"/> sink before running.
/// </summary>
/// <typeparam name="TProgress">The type of progress report emitted by the loader.</typeparam>
public interface IPipelineWithLoadProgress<TProgress> : IPipeline
    where TProgress : notnull
{
    /// <summary>
    /// Binds an <see cref="IProgress{TProgress}"/> sink to the loader. The returned pipeline
    /// is a plain <see cref="IPipeline"/> — progress can only be set once.
    /// </summary>
    /// <param name="progress">The progress sink to forward reports to.</param>
    /// <returns>The same pipeline, with progress bound, typed as <see cref="IPipeline"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="progress"/> is <see langword="null"/>.</exception>
    IPipeline WithProgress(IProgress<TProgress> progress);
}
