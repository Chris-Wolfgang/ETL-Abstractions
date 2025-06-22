using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions;

namespace Example2_WithCancellationToken.ETL
{
    internal class IntToStringTransformer : ITransformWithCancellationAsync<int, string>
    {
        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            Console.WriteLine($"{ConsoleColors.Green}Transforming{ConsoleColors.Reset} integers to strings asynchronously...\n");
            
            await foreach (var item in items.WithCancellation(token))
            {
                // Throw exception if cancellation is requested
                // See Example3-ExtractorWithGracefulCancellation for a more graceful cancellation approach
                token.ThrowIfCancellationRequested();

                Console.WriteLine($"Transforming integer {item} to string.");
                await Task.Delay(50); // Simulate some delay for transformation
                yield return item.ToString();
            }
            
            Console.WriteLine($"{ConsoleColors.Green}Transformation{ConsoleColors.Reset} completed.\n");
        }
    }
}
