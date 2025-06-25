using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example6_ReducingDuplicateCode.ETL
{
    internal class IntToStringTransformer : ITransformWithProgressAndCancellationAsync<int, string, EtlProgress>
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



        public IAsyncEnumerable<string> TransformAsync
            (
                IAsyncEnumerable<int> items
            )
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }


            return WorkerAsync(items, null, CancellationToken.None);
        }



        public IAsyncEnumerable<string> TransformAsync
            (
                IAsyncEnumerable<int> items, 
                CancellationToken token
            )
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }


            return WorkerAsync(items, null, token);
        }



        public IAsyncEnumerable<string> TransformAsync
            (
                IAsyncEnumerable<int> items, 
                IProgress<EtlProgress> progress
            )
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



        public IAsyncEnumerable<string> TransformAsync
        (
            IAsyncEnumerable<int> items,
            IProgress<EtlProgress> progress,
            CancellationToken token
        )
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



        private async IAsyncEnumerable<string> WorkerAsync
        (
            IAsyncEnumerable<int> items,
            IProgress<EtlProgress>? progress,
            [EnumeratorCancellation] CancellationToken token
        )
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Console.WriteLine($"{ConsoleColors.Green}Transforming{ConsoleColors.Reset} integers to strings asynchronously...\n");

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
                // You can either throw an exception if cancellation is requested 
                // token.ThrowIfCancellationRequested();

                // or gracefully handle it.
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"{ConsoleColors.Red}Transformation cancelled{ConsoleColors.Reset}.");
                    yield break;
                }

                Console.WriteLine($"Transforming integer {item} to string.");
                await Task.Delay(50); // Simulate some delay for transformation
                yield return item.ToString();
                count = Interlocked.Increment(ref count);

            }

            progress?.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count

            Console.WriteLine($"{ConsoleColors.Green}Transformation{ConsoleColors.Reset} completed.\n");
        }
    }
}
