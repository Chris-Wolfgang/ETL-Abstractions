using Wolfgang.Etl.Abstractions;

namespace Example5a_ExtractorWithProgressAndCancellation.ETL
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



        public async Task LoadAsync(IAsyncEnumerable<string> items)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));

            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            await foreach (var item in items)
            {
                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
            }
            
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }



        public async Task LoadAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {

            ArgumentNullException.ThrowIfNull(items, nameof(items));
            
            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            await foreach (var item in items.WithCancellation(token))
            {
                // You can either throw an exception if cancellation is requested 
                // token.ThrowIfCancellationRequested();
                
                // or gracefully handle it.
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"{ConsoleColors.Red}Loading cancelled{ConsoleColors.Reset}.");
                    return;
                }

                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
            }

            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }



        public async Task LoadAsync(IAsyncEnumerable<string> items, IProgress<EtlProgress> progress)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));
            ArgumentNullException.ThrowIfNull(progress, nameof(progress));

            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            var count = 0;
            await using var timer = new Timer
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



        public async Task LoadAsync(IAsyncEnumerable<string> items, IProgress<EtlProgress> progress, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));
            ArgumentNullException.ThrowIfNull(progress, nameof(progress));

            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} data to console asynchronously...\n");

            var count = 0;
            await using var timer = new Timer
            (
                _ => progress.Report(new EtlProgress(Volatile.Read(ref count))),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(_progressInterval) // Use the configured progress interval
            );


            await foreach (var item in items)
            {
                token.ThrowIfCancellationRequested();

                Console.WriteLine($"Loading item: {item}\n");
                await Task.Delay(50); // Simulate some delay for loading
                count = Interlocked.Increment(ref count);

            }

            progress.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count


            Console.WriteLine($"{ConsoleColors.Green}Loading{ConsoleColors.Reset} completed.\n");
        }

    }
}
