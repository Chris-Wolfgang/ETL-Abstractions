#nullable enable
﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example6_ReducingDuplicateCode.ETL;
internal class FibonacciExtractor : IExtractWithProgressAndCancellationAsync<int, EtlProgress>
{
    private int _progressInterval = 1_000;


    /// <summary>
    /// The number of milliseconds between progress updates.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Value cannot be less than 1.</exception>
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



    public IAsyncEnumerable<int> ExtractAsync()
    {
        return WorkerAsync(progress: null, token: CancellationToken.None);
    }



    public IAsyncEnumerable<int> ExtractAsync(CancellationToken token)
    {
        return WorkerAsync(progress: null, token: token);
    }



    public IAsyncEnumerable<int> ExtractAsync(IProgress<EtlProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        return WorkerAsync(progress: progress, token: CancellationToken.None);
    }



    public IAsyncEnumerable<int> ExtractAsync
        (
            IProgress<EtlProgress> progress, 
            CancellationToken token
        )
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        return WorkerAsync(progress: progress, token: token);
    }



    private async IAsyncEnumerable<int> WorkerAsync
    (
        IProgress<EtlProgress>? progress, 
        [EnumeratorCancellation] CancellationToken token
    )
    {
        Console.WriteLine($"{ConsoleColors.Green}Extracting{ConsoleColors.Reset} Fibonacci numbers asynchronously...\n");

        var count = 0;
        using var timer = new Timer
        (
            state => progress?.Report(new EtlProgress(Volatile.Read(ref count))),
            state: null,
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
            await Task.Delay(100, token); // Simulate asynchronous operation
        }

        progress?.Report(new EtlProgress(Volatile.Read(ref count))); // Report final count

        Console.WriteLine($"{ConsoleColors.Green}Extraction{ConsoleColors.Reset} completed.\n");
    }
}
