using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions;

namespace Example3_WithGracefulCancellation.ETL
{
    internal class FibonacciExtractor : IExtractWithCancellationAsync<int>
    {
        public async IAsyncEnumerable<int> ExtractAsync()
        {
            Console.WriteLine($"{ConsoleColors.Green}Extracting{ConsoleColors.Reset} Fibonacci numbers asynchronously...\n");

            var current = 1;
            var previous = 0;
            for (var x = 0; x < 10; ++x)
            {
                Console.WriteLine($"Extracting Fibonacci number {x + 1}: {current}");
                yield return current;
                var temp = current;
                current += previous;
                previous = temp;
                await Task.Delay(100); // Simulate asynchronous operation
            }

            Console.WriteLine($"{ConsoleColors.Green}Extraction{ConsoleColors.Reset} completed.\n");
        }



        public async IAsyncEnumerable<int> ExtractAsync([EnumeratorCancellation] CancellationToken token)
        {
            Console.WriteLine($"{ConsoleColors.Green}Extracting{ConsoleColors.Reset} Fibonacci numbers asynchronously...\n");

            var current = 1;
            var previous = 0;
            for (var x = 0; x < 10; ++x)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"{ConsoleColors.Red}Extraction cancelled{ConsoleColors.Reset}.");
                    yield break; // Exit the method if cancellation is requested
                }

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
