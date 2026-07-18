using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Entry point for building a generic, format-agnostic ETL pipeline. Start from a source — either a
/// built-in <see cref="From{T}(IAsyncEnumerable{T})"/> / <see cref="From{T, TProgress}(ExtractorBase{T, TProgress})"/>
/// factory, or a format-specific factory hung off <see cref="Source"/> by a format package — chain
/// append transformer stages on <see cref="IEtlPipeline{T}"/>, terminate with a sink, then call
/// <see cref="IEtlPipelineSink.RunAsync(IProgress{EtlPipelineProgress}, CancellationToken)"/>.
/// </summary>
/// <remarks>
/// <para>
/// The name <c>EtlPipeline</c> (rather than <c>Pipeline</c>) avoids clashing with the fluent
/// <see cref="Pipeline"/> extract/transform/load builder in this same namespace and with
/// <c>System.IO.Pipelines</c>.
/// </para>
/// <para>
/// The core exposes only the plumbing — a source, <see cref="IEtlPipeline{T}.Through{TOut}(ITransformAsync{T, TOut})"/>
/// for appending transformer stages, and a sink. The LINQ-flavored operators (<c>Where</c>,
/// <c>Select</c>, <c>Distinct</c>, …) are extension methods shipped by <c>Wolfgang.Etl.Transformers</c>,
/// which already owns those transformers.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// await EtlPipeline.From(records)
///     .Through(new WhereTransformer&lt;Order&gt;(r =&gt; r.Amount &gt; 0))
///     .To(sqlLoader)
///     .RunAsync(progress, token);
/// </code>
/// </example>
public static class EtlPipeline
{
    /// <summary>
    /// The sentinel that format packages extend with source factories, enabling the
    /// <c>EtlPipeline.Source.CsvExtractor&lt;T&gt;(...)</c> shape. See <see cref="EtlPipelineSource"/>.
    /// </summary>
    public static EtlPipelineSource Source { get; } = new();


    /// <summary>
    /// Begins a pipeline from an existing asynchronous stream — the generic escape hatch for any
    /// source the caller already has as an <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item produced by the source.</typeparam>
    /// <param name="source">The stream that seeds the pipeline.</param>
    /// <returns>An <see cref="IEtlPipeline{T}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEtlPipeline<T> From<T>(IAsyncEnumerable<T> source)
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
    /// <param name="extractor">The extractor that seeds the pipeline.</param>
    /// <returns>An <see cref="IEtlPipeline{T}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="extractor"/> is <see langword="null"/>.</exception>
    public static IEtlPipeline<T> From<T, TProgress>(ExtractorBase<T, TProgress> extractor)
        where T : notnull
        where TProgress : notnull
    {
        if (extractor is null)
        {
            throw new ArgumentNullException(nameof(extractor));
        }

        return EtlPipelineImpl<T>.FromStream((_, token) => extractor.ExtractAsync(token));
    }
}
