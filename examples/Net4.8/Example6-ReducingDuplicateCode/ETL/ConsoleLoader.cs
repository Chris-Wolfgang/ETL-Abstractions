using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example6_ReducingDuplicateCode.ETL
{
    internal class ConsoleLoader : ILoadWithProgressAndCancellationAsync<string, EtlProgress>
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



        public Task LoadAsync(IAsyncEnumerable<string> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return WorkerAsync(items, null, CancellationToken.None);
        }



        public Task LoadAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return WorkerAsync(items, null, token);
        }



        public Task LoadAsync(IAsyncEnumerable<string> items, IProgress<EtlProgress> progress)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (progress is null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            return WorkerAsync(items, progress, CancellationToken.None);
        }



        public Task LoadAsync(IAsyncEnumerable<string> items, IProgress<EtlProgress> progress, CancellationToken token)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (progress is null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            return WorkerAsync(items, progress, token);
        }



        private async Task WorkerAsync
        (
            IAsyncEnumerable<string> items, 
            IProgress<EtlProgress>? progress, 
            CancellationToken token
        )
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            var count = 0;
            using var timer = new Timer
            (
                _ => progress?.Report(new EtlProgress(Volatile.Read(ref count))),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(_progressInterval) // Use the configured progress interval
            );


            await foreach (var item in items.WithCancellation(token))
            {
                token.ThrowIfCancellationRequested();

                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
                count = Interlocked.Increment(ref count);

            }

            progress?.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count


            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }


    }
}
