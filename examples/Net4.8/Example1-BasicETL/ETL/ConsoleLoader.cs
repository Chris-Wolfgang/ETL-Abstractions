using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example1_BasicETL.ETL
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
