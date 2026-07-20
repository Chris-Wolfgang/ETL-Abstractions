namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A sentinel target for source-factory extension methods. C# cannot attach extension methods to a
/// static class, so <see cref="EtlPipeline.Source"/> exposes an instance of this type for format
/// packages to extend — for example a CSV package adds
/// <c>public static ICsvExtractorBuilder&lt;T&gt; CsvExtractor&lt;T&gt;(this EtlPipelineSource source, string path)</c>,
/// enabling <c>EtlPipeline.Source.CsvExtractor&lt;Order&gt;("orders.csv")</c>.
/// </summary>
public sealed class EtlPipelineSource
{
    internal EtlPipelineSource()
    {
    }
}
