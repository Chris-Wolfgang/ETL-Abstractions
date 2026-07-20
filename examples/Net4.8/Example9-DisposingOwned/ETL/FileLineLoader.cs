using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Example9_DisposingOwned.ETL
{
    // A sink that writes each record as a line to a stream it was GIVEN — it does not own or
    // dispose the stream (note leaveOpen: true). That ownership belongs to whoever opened it.
    // In a real format package this is what a path-based sink factory wraps, handing stream
    // cleanup to DisposingOwned.
    internal sealed class FileLineLoader : LoaderBase<string, Report>
    {
        private readonly Stream _stream;

        public FileLineLoader(Stream stream)
        {
            _stream = stream;
        }

        protected override async Task LoadWorkerAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            using (var writer = new StreamWriter(_stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true))
            {
                await foreach (var item in items.WithCancellation(token))
                {
                    token.ThrowIfCancellationRequested();
                    await writer.WriteLineAsync(item);
                    IncrementCurrentItemCount();
                }
            }
        }

        protected override Report CreateProgressReport()
        {
            return new Report(CurrentItemCount);
        }
    }
}
