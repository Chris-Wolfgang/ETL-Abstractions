using Wolfgang.Etl.Abstractions;

namespace Example8_EtlPipeline.ETL;

// Stage 1: string -> int
internal sealed class ParseIntTransformer : ITransformAsync<string, int>
{
    public async IAsyncEnumerable<int> TransformAsync(IAsyncEnumerable<string> items)
    {
        await foreach (var item in items)
        {
            var value = int.Parse(item);
            Console.WriteLine($"{ConsoleColors.Green}parse {ConsoleColors.Reset} \"{item}\" -> {value}");
            yield return value;
        }
    }
}
