using Wolfgang.Etl.Abstractions;

namespace Example8_EtlPipeline.ETL;

// Stage 3: double -> string
internal sealed class FormatTransformer : ITransformAsync<double, string>
{
    public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<double> items)
    {
        await foreach (var item in items)
        {
            yield return $"value = {item:F1}";
        }
    }
}
