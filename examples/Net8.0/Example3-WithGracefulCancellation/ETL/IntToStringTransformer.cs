using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions;

namespace Example3_WithGracefulCancellation.ETL
{
    internal class IntToStringTransformer : ITransformWithCancellationAsync<int, string>
    {

        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items)
        {
            Console.WriteLine($"{ConsoleColors.Green}Transforming{ConsoleColors.Reset} integers to strings asynchronously...\n");

            await foreach (var item in items)
            {
                Console.WriteLine($"Transforming integer {item} to string.");
                await Task.Delay(50); // Simulate some delay for transformation
                yield return item.ToString();
            }

            Console.WriteLine($"{ConsoleColors.Green}Transformation{ConsoleColors.Reset} completed.\n");
        }



        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            Console.WriteLine($"{ConsoleColors.Green}Transforming{ConsoleColors.Reset} integers to strings asynchronously...\n");
            
            await foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"{ConsoleColors.Red}Extraction cancelled{ConsoleColors.Reset}.");
                    yield break; // Exit the method if cancellation is requested
                }

                Console.WriteLine($"Transforming integer {item} to string.");
                await Task.Delay(50); // Simulate some delay for transformation
                yield return item.ToString();
            }
            
            Console.WriteLine($"{ConsoleColors.Green}Transformation{ConsoleColors.Reset} completed.\n");
        }
    }
}
