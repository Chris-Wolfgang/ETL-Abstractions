using System;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A terminated, runnable ETL pipeline. Obtained from an <see cref="IExtractStage{TSource}"/> or
/// <see cref="ITransformStage{TSource}"/> <c>Load</c> overload.
/// </summary>
/// <remarks>
/// <para>
/// A pipeline is one-shot: calling <see cref="RunAsync()"/> or
/// <see cref="RunAsync(CancellationToken)"/> a second time on the same instance throws
/// <see cref="InvalidOperationException"/>. Construct a new pipeline for each run.
/// </para>
/// <para>
/// <b>Exception handling.</b> The pipeline does not catch, wrap, or aggregate exceptions.
/// Any exception thrown by an extractor, transformer, or loader — including
/// <see cref="OperationCanceledException"/> from cancellation — propagates unchanged to the
/// caller of <c>RunAsync</c>. Wrap the call in <c>try</c>/<c>catch</c> to handle failures at
/// the call site:
/// </para>
/// <code>
/// try
/// {
///     await Pipeline.Extract(e).Transform(t).Load(l).RunAsync(token);
/// }
/// catch (OperationCanceledException)
/// {
///     // cancellation
/// }
/// catch (Exception ex)
/// {
///     logger.LogError(ex, "Pipeline failed");
///     throw;
/// }
/// </code>
/// <para>
/// After an exception, the stage instances the caller constructed remain valid and inspectable.
/// <c>CurrentItemCount</c>, <c>CurrentSkippedItemCount</c>, and similar properties reflect
/// progress up to the point of failure and can be read for diagnostics.
/// </para>
/// </remarks>
public interface IPipeline
{
    /// <summary>
    /// Assigns a name to the pipeline for diagnostics. Purely informational; callers may
    /// ignore it. Repeated calls replace the previous value.
    /// </summary>
    /// <param name="name">The name to assign.</param>
    /// <returns>The same pipeline, for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    IPipeline WithName(string name);


    /// <summary>
    /// The name assigned via <see cref="WithName"/>, or <see langword="null"/> if none has been set.
    /// </summary>
    string? Name { get; }


    /// <summary>
    /// Opts the pipeline into disposing its stages when the run completes (whether it succeeds or
    /// throws). After <see cref="RunAsync()"/> finishes, each stage (extractor, every transformer,
    /// and the loader) that implements <see cref="System.IAsyncDisposable"/> is disposed via
    /// <c>DisposeAsync</c>; otherwise, if it implements <see cref="IDisposable"/>, via
    /// <c>Dispose</c>; stages that implement neither are skipped.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default behavior is the opposite — the pipeline owns nothing and the caller is
    /// responsible for disposing the stages it constructed. Call this only when the stages are
    /// owned by the call site and should not outlive the run (the common short-lived case), to
    /// avoid wrapping every stage in its own <c>using</c>.
    /// </para>
    /// <para>
    /// Every stage is disposed even if an earlier disposal throws; any exceptions thrown <em>while
    /// disposing</em> are collected and surfaced as an <see cref="AggregateException"/>. If the run
    /// itself threw, that original exception propagates and disposal still runs.
    /// </para>
    /// </remarks>
    /// <returns>The same pipeline, for fluent chaining.</returns>
    IPipeline DisposeStagesOnCompletion();


    /// <summary>
    /// Runs the pipeline to completion with no cancellation token.
    /// </summary>
    /// <returns>A task that completes when the loader has finished consuming the stream.</returns>
    /// <exception cref="InvalidOperationException">The pipeline has already been run.</exception>
    Task RunAsync();


    /// <summary>
    /// Runs the pipeline to completion, forwarding <paramref name="token"/> to every stage.
    /// </summary>
    /// <param name="token">A cancellation token observed by every stage.</param>
    /// <returns>A task that completes when the loader has finished consuming the stream.</returns>
    /// <exception cref="InvalidOperationException">The pipeline has already been run.</exception>
    Task RunAsync(CancellationToken token);
}
