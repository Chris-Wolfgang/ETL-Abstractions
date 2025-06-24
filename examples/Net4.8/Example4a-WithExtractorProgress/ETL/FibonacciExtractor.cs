using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example4a_WithExtractorProgress.ETL
{
    internal class FibonacciExtractor : IExtractAsync<int>, IExtractWithProgressAsync<int, EtlProgress>
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



        public async IAsyncEnumerable<int> ExtractAsync(IProgress<EtlProgress> progress)
        {
            Console.WriteLine($"{ConsoleColors.Green}Extracting{ConsoleColors.Reset} Fibonacci numbers asynchronously...\n");

            var count = 0;
            using var timer = new Timer
            ( 
                _ => progress.Report(new EtlProgress(Volatile.Read(ref count))),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(_progressInterval) // Use the configured progress interval
            );

            var current = 1;
            var previous = 0;
            for (var x = 0; x < 10; ++x)
            {
                Console.WriteLine($"Extracting Fibonacci number {x + 1}: {current}");
                yield return current;
                count = Interlocked.Increment(ref count);

                var temp = current;
                current += previous;
                previous = temp;
                await Task.Delay(100); // Simulate asynchronous operation
            }

            progress.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count

            Console.WriteLine($"{ConsoleColors.Green}Extraction{ConsoleColors.Reset} completed.\n");
        }
    }
}
