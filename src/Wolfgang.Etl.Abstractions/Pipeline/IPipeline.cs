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
    /// Stages are disposed in <strong>reverse construction order</strong> (loader → transformers
    /// → extractor), matching the LIFO convention of nested <c>using</c>/<c>await using</c> blocks
    /// and DI-container scope disposal.
    /// </para>
    /// <para>
    /// Every stage is disposed even if an earlier disposal throws. What surfaces to the caller
    /// depends on which failures happened:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Run succeeded, no disposal errors → <see cref="RunAsync()"/> returns normally.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Run succeeded, one or more stages threw while being disposed → those exceptions are
    ///       collected and surfaced as a single <see cref="AggregateException"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Run threw (with or without additional disposal failures) → the <em>run's</em>
    ///       exception propagates unchanged (preserving its original stack trace), disposal still
    ///       runs to completion, but any disposal exceptions are suppressed. The run failure is
    ///       treated as the primary signal because it is what the caller expected to succeed;
    ///       callers who need to observe disposal failures on the failing-run path should log
    ///       them inside their own stages' <c>Dispose</c>/<c>DisposeAsync</c> implementations.
    ///     </description>
    ///   </item>
    /// </list>
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
