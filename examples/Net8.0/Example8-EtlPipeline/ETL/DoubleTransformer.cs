using Wolfgang.Etl.Abstractions;

namespace Example8_EtlPipeline.ETL;

// Stage 2: int -> int (a same-type stage — chaining doesn't require a type change)
internal sealed class DoubleTransformer : ITransformAsync<int, int>
{
    public async IAsyncEnumerable<int> TransformAsync(IAsyncEnumerable<int> items)
    {
        await foreach (var item in items)
        {
            var doubled = item * 2;
            Console.WriteLine($"{ConsoleColors.Green}double{ConsoleColors.Reset} {item} -> {doubled}");
            yield return doubled;
        }
    }
}
