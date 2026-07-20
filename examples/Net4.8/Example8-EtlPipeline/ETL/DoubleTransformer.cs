using System;
using System.Collections.Generic;
using Wolfgang.Etl.Abstractions;

namespace Example8_EtlPipeline.ETL
{
    // Stage 2: int -> double
    internal sealed class DoubleTransformer : ITransformAsync<int, double>
    {
        public async IAsyncEnumerable<double> TransformAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var item in items)
            {
                var doubled = item * 2.0;
                Console.WriteLine($"{ConsoleColors.Green}double{ConsoleColors.Reset} {item} -> {doubled:F1}");
                yield return doubled;
            }
        }
    }
}
