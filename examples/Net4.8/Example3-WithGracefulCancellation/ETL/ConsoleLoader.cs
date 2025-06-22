using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example3_WithGracefulCancellation.ETL
{
    internal class ConsoleLoader : ILoadWithCancellationAsync<string>
    {
        public async Task LoadAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            await foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"{ConsoleColors.Red}Loading cancelled.{ConsoleColors.Reset}");
                    return; // Exit gracefully if cancellation is requested
                }

                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
            }
            
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }
    }
}
