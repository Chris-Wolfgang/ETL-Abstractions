using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class ConsoleLoaderWithProgress : ILoadWithProgressAsync<string, EtlProgress>
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
            await foreach (var item in items)
            {
                Console.WriteLine(item);
            }
        }



        public async Task LoadAsync(IAsyncEnumerable<string> items, IProgress<EtlProgress> progress)
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
                Console.WriteLine($"Loading item: {item}\n");
                await Task.Yield(); // Simulate some delay for loading
                count = Interlocked.Increment(ref count);

            }

            progress.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count
        }
    }
}
