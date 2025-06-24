using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example5a_ExtractorWithProgressAndCancellation.ETL
{
    internal class FibonacciExtractor : IExtractWithProgressAndCancellationAsync<int, EtlProgress>
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



        public async IAsyncEnumerable<int> ExtractAsync([EnumeratorCancellation] CancellationToken token)
        {
            Console.WriteLine($"{ConsoleColors.Green}Extracting{ConsoleColors.Reset} Fibonacci numbers asynchronously...\n");

            var current = 1;
            var previous = 0;
            for (var x = 0; x < 10; ++x)
            {
                // Throw exception if cancellation is requested
                // See Example3-ExtractorWithGracefulCancellation for a more graceful cancellation approach
                token.ThrowIfCancellationRequested();

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
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

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



        public async IAsyncEnumerable<int> ExtractAsync(IProgress<EtlProgress> progress, [EnumeratorCancellation] CancellationToken token)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

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
                // You can either throw an exception if cancellation is requested 
                // token.ThrowIfCancellationRequested();

                // or gracefully handle it.
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"{ConsoleColors.Red}Extraction cancelled{ConsoleColors.Reset}.");
                    yield break;
                }

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
