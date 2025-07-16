using Wolfgang.Etl.Abstractions;

namespace Example2_WithCancellationToken.ETL
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

            await foreach (var item in items.WithCancellation(token))
            {
                // Throw exception if cancellation is requested
                // See Example3-ExtractorWithGracefulCancellation for a more graceful cancellation approach
                token.ThrowIfCancellationRequested();

                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
            }
            
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }
    }
}
