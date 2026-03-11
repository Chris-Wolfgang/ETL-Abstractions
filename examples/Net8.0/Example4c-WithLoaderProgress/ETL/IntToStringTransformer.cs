using Wolfgang.Etl.Abstractions;

namespace Example4c_WithLoaderProgress.ETL
{
    internal class IntToStringTransformer 
        : ITransformAsync<int, string>, ITransformWithProgressAsync<int, string, EtlProgress>
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



        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items)
        {
            Console.WriteLine($"{ConsoleColors.Green}Transforming{ConsoleColors.Reset} integers to strings asynchronously...\n");
            
            await foreach (var item in items)
            {
                Console.WriteLine($"Transforming integer {item} to string.");
                await Task.Delay(50); // Simulate some delay for transformation
                yield return item.ToString();
            }
            
            Console.WriteLine($"{ConsoleColors.Green}Transformation{ConsoleColors.Reset} completed.\n");
        }



        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items, IProgress<EtlProgress> progress)
        {
            Console.WriteLine($"{ConsoleColors.Green}Transforming{ConsoleColors.Reset} integers to strings asynchronously...\n");

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
                Console.WriteLine($"Transforming integer {item} to string.");
                await Task.Delay(50); // Simulate some delay for transformation
                yield return item.ToString();
                count = Interlocked.Increment(ref count);

            }

            progress.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count

            Console.WriteLine($"{ConsoleColors.Green}Transformation{ConsoleColors.Reset} completed.\n");
        }
    }
}
