using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class FibonacciExtractorWithProgressAndCancellation : IExtractWithProgressAndCancellationAsync<int, EtlProgress>
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
            var current = 1;
            var previous = 0;
            for (var x = 0; x < 10; ++x)
            {
                yield return current;
                var temp = current;
                current += previous;
                previous = temp;
                await Task.Yield(); // Simulate some delay for loading
            }
        }



        public async IAsyncEnumerable<int> ExtractAsync([EnumeratorCancellation] CancellationToken token)
        {
            var current = 1;
            var previous = 0;
            for (var x = 0; x < 10; ++x)
            {
                token.ThrowIfCancellationRequested();

                yield return current;
                var temp = current;
                current += previous;
                previous = temp;
                await Task.Yield(); // Simulate some delay for loading
            }
        }



        public async IAsyncEnumerable<int> ExtractAsync(IProgress<EtlProgress> progress)
        {
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
                yield return current;
                count = Interlocked.Increment(ref count);

                var temp = current;
                current += previous;
                previous = temp;
                await Task.Yield(); // Simulate some delay for loading
            }

            progress.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count
        }



        public async IAsyncEnumerable<int> ExtractAsync(IProgress<EtlProgress> progress, [EnumeratorCancellation] CancellationToken token)
        {
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
                token.ThrowIfCancellationRequested();

                yield return current;
                count = Interlocked.Increment(ref count);

                var temp = current;
                current += previous;
                previous = temp;
                await Task.Yield(); // Simulate some delay for loading
            }

            progress.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count
        }
    }
}
