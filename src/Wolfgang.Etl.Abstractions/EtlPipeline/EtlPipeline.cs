using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Entry point for building a generic, format-agnostic ETL pipeline. Obtain a fresh builder with
/// <see cref="Create"/>, start from a source — either a built-in <see cref="From{T}(IAsyncEnumerable{T})"/> /
/// <see cref="From{T, TProgress}(ExtractorBase{T, TProgress})"/> factory, or a format-specific factory
/// hung off the <see cref="EtlPipeline"/> instance by a format package — chain append transformer stages
/// on <see cref="IEtlPipeline{T}"/>, terminate with a sink, then call
/// <see cref="IEtlPipelineSink.RunAsync(IProgress{EtlPipelineProgress}, CancellationToken)"/>.
/// </summary>
/// <remarks>
/// <para>
/// The name <c>EtlPipeline</c> (rather than <c>Pipeline</c>) avoids clashing with the fluent
/// <see cref="Pipeline"/> extract/transform/load builder in this same namespace and with
/// <c>System.IO.Pipelines</c>.
/// </para>
/// <para>
/// <see cref="Create"/> returns a fresh <see cref="EtlPipeline"/> seed: it carries no data itself and
/// exists to give source factories a strongly-typed receiver. Format packages extend the instance with
/// class-named factories — for example a CSV package adds
/// <c>public static ICsvExtractorBuilder&lt;T&gt; CsvExtractor&lt;T&gt;(this EtlPipeline pipeline, string path)</c>,
/// enabling <c>EtlPipeline.Create().CsvExtractor&lt;Order&gt;("orders.csv")</c>.
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
/// await EtlPipeline.Create()
///     .From(records)
///     .Through(new WhereTransformer&lt;Order&gt;(r =&gt; r.Amount &gt; 0))
///     .To(sqlLoader)
///     .RunAsync(progress, token);
/// </code>
/// </example>
public sealed class EtlPipeline
{
    private EtlPipeline()
    {
    }


    /// <summary>
    /// Creates a new, empty <see cref="EtlPipeline"/> builder to start a fluent pipeline chain.
    /// </summary>
    /// <returns>A fresh builder instance.</returns>
    public static EtlPipeline Create() => new();


    /// <summary>
    /// Begins a pipeline from an existing asynchronous stream — the generic escape hatch for any
    /// source the caller already has as an <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item produced by the source.</typeparam>
    /// <param name="source">The stream that seeds the pipeline.</param>
    /// <returns>An <see cref="IEtlPipeline{T}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public IEtlPipeline<T> From<T>(IAsyncEnumerable<T> source)
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
    public IEtlPipeline<T> From<T, TProgress>(ExtractorBase<T, TProgress> extractor)
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
