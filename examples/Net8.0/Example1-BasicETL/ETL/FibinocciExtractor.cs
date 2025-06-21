using Wolfgang.Etl.Abstractions;

namespace Example1_BasicETL.ETL
{
    internal class FibonacciExtractor : IExtractAsync<int>
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
                await Task.Yield(); // Simulate asynchronous operation
            }

            Console.WriteLine($"{ConsoleColors.Green}Extraction{ConsoleColors.Reset} completed.\n");
        }
    }
}
