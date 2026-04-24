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
