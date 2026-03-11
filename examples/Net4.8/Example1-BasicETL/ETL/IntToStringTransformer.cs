using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example1_BasicETL.ETL
{
    internal class IntToStringTransformer : ITransformAsync<int, string>
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
    }
}
