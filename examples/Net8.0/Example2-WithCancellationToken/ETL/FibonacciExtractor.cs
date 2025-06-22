using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions;

namespace Example2_WithCancellationToken.ETL
{
    internal class FibonacciExtractor : IExtractWithCancellationAsync<int>
    {
        public async IAsyncEnumerable<int> ExtractAsync([EnumeratorCancellation] CancellationToken token)
        {
            Console.WriteLine($"{ConsoleColors.Green}Extracting{ConsoleColors.Reset} Fibonacci numbers asynchronously...\n");

            var current = 1;
            var previous = 0;
            for (var x = 0; x < 10; ++x)
            {
                // Throw exception if cancellation is requested
                // See Example3-ExtractorWithGracefulCancellation for a more graceful cancellation approach
                token.ThrowIfCancellationRequested();

                Console.WriteLine($"Extracting Fibonacci number {x + 1}: {current}");
                yield return current;
                var temp = current;
                current += previous;
                previous = temp;
                await Task.Delay(100); // Simulate asynchronous operation
            }

            Console.WriteLine($"{ConsoleColors.Green}Extraction{ConsoleColors.Reset} completed.\n");
        }
    }
}
