using System.Collections.Generic;
using Wolfgang.Etl.Abstractions;

namespace Example8_EtlPipeline.ETL
{
    // Stage 3: int -> string
    internal sealed class FormatTransformer : ITransformAsync<int, string>
    {
        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var item in items)
            {
                yield return $"value = {item}";
            }
        }
    }
}
