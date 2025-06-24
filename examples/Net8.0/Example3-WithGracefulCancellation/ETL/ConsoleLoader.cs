using Wolfgang.Etl.Abstractions;

namespace Example3_WithGracefulCancellation.ETL
{
    internal class ConsoleLoader : ILoadWithCancellationAsync<string>
    {

        public async Task LoadAsync(IAsyncEnumerable<string> items)
        {
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            await foreach (var item in items)
            {
                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
            }

            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }



        public async Task LoadAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            await foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"{ConsoleColors.Red}Extraction cancelled{ConsoleColors.Reset}.");
                    return; // Exit the method if cancellation is requested
                }

                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
            }
            
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }
    }
}
