using System;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Entry point for building a generic, format-agnostic ETL pipeline. Obtain a fresh builder with
/// <see cref="Create"/>, start from a source — either a built-in <c>From</c> factory (see
/// <see cref="EtlPipelineSourceExtensions"/>) or a format-specific factory hung off the
/// <see cref="EtlPipeline"/> instance by a format package — append transformer stages on
/// <see cref="IEtlPipeline{T}"/>, terminate with a sink, then call
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
/// exists to give source factories a strongly-typed receiver. The built-in <c>From</c> factories and
/// any format-package factories are extension methods on this type — for example a CSV package adds
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
}
