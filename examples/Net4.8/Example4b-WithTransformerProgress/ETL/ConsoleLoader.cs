using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example4b_WithTransformerProgress.ETL
{
    internal class ConsoleLoader : ILoadAsync<string>, ILoadWithProgressAsync<string, EtlProgress>
    {


        private int _progressInterval = 1_000;

        /// <summary>
        /// The number of milliseconds between progress updates.
        /// </summary>
        public int ProgressInterval
        {
            get => _progressInterval;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Progress interval must be greater than 0.");
                }
                _progressInterval = value;
            }
        }



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



        public async Task LoadAsync(IAsyncEnumerable<string> items, IProgress<EtlProgress> progress)
        {
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            var count = 0;
            using var timer = new Timer
            (
                _ => progress.Report(new EtlProgress(Volatile.Read(ref count))),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(_progressInterval) // Use the configured progress interval
            );


            await foreach (var item in items)
            {
                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
                count = Interlocked.Increment(ref count);

            }

            progress.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count


            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }
    }
}
