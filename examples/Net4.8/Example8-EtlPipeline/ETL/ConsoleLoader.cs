using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example8_EtlPipeline.ETL
{
    // Sink: the pipeline's To(...) terminator requires a LoaderBase<T, TProgress>.
    internal sealed class ConsoleLoader : LoaderBase<string, Report>
    {
        protected override async Task LoadWorkerAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token))
            {
                token.ThrowIfCancellationRequested();
                Console.WriteLine($"{ConsoleColors.Green}load  {ConsoleColors.Reset} {item}");
                IncrementCurrentItemCount();
            }
        }


        protected override Report CreateProgressReport()
        {
            return new Report(CurrentItemCount);
        }
    }
}
