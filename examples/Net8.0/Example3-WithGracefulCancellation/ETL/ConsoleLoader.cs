using Example3_ExtractorWithGracefulCancellation;
using Wolfgang.Etl.Abstractions;

namespace Example3_WithGracefulCancellation.ETL
{
    internal class ConsoleLoader : ILoadAsync<string>
    {
        public async Task LoadAsync(IAsyncEnumerable<string> source)
        {
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            await foreach (var item in source)
            {
                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
            }
            
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }
    }
}
