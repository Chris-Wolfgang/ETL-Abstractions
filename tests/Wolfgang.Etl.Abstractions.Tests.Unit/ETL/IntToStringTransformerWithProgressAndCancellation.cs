using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class IntToStringTransformerWithProgressAndCancellation : ITransformWithProgressAndCancellationAsync<int, string, EtlProgress>
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



        public async IAsyncEnumerable<string> TransformAsync
            (
                IAsyncEnumerable<int> items
            )
        {
            
            await foreach (var item in items)
            {
                Console.WriteLine($"Transforming integer {item} to string.");
                await Task.Yield(); // Simulate some delay for loading
                yield return item.ToString();
            }
        }



        public async IAsyncEnumerable<string> TransformAsync
            (
                IAsyncEnumerable<int> items, 
                [EnumeratorCancellation] CancellationToken token
            )
        {
            await foreach (var item in items.WithCancellation(token))
            {
                token.ThrowIfCancellationRequested();

                Console.WriteLine($"Transforming integer {item} to string.");
                await Task.Yield(); // Simulate some delay for loading
                yield return item.ToString();
            }
        }



        public async IAsyncEnumerable<string> TransformAsync
            (
                IAsyncEnumerable<int> items, 
                IProgress<EtlProgress> progress
            )
        {
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
                await Task.Yield(); // Simulate some delay for loading
                yield return item.ToString();
                count = Interlocked.Increment(ref count);

            }

            progress.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count
        }



        public async IAsyncEnumerable<string> TransformAsync
        (
            IAsyncEnumerable<int> items,
            IProgress<EtlProgress> progress,
            [EnumeratorCancellation] CancellationToken token
        )
        {
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
                token.ThrowIfCancellationRequested();

                await Task.Yield(); // Simulate some delay for loading
                yield return item.ToString();
                count = Interlocked.Increment(ref count);

            }
            progress.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count
        }
    }
}
