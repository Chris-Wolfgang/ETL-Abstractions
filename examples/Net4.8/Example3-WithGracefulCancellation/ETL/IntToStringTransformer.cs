using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example3_WithGracefulCancellation.ETL
{
    internal class IntToStringTransformer : ITransformWithCancellationAsync<int, string>
    {
        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            Console.WriteLine($"{ConsoleColors.Green}Transforming{ConsoleColors.Reset} integers to strings asynchronously...\n");
            
            await foreach (var item in items.WithCancellation(token))
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"{ConsoleColors.Red}Loading cancelled.{ConsoleColors.Reset}");
                    yield break; // Exit gracefully if cancellation is requested
                }

                Console.WriteLine($"Transforming integer {item} to string.");
                await Task.Delay(50); // Simulate some delay for transformation
                yield return item.ToString();
            }
            
            Console.WriteLine($"{ConsoleColors.Green}Transformation{ConsoleColors.Reset} completed.\n");
        }
    }
}
