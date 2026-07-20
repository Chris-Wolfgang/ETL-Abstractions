using Example9_DisposingOwned.ETL;
using Wolfgang.Etl.Abstractions;

namespace Example9_DisposingOwned;

internal class Program
{
    private static async Task Main()
    {
        Console.WriteLine($"{ConsoleColors.Green}.NET Version: {Environment.Version}{ConsoleColors.Reset}\n");

        Console.WriteLine(
            $"{ConsoleColors.Yellow}Running a pipeline whose sink writes to a file the caller opened, " +
            $"then letting DisposingOwned close it...{ConsoleColors.Reset}\n\n");

        var path = Path.Combine(Path.GetTempPath(), $"example9-{Guid.NewGuid():N}.txt");

        // The pipeline builder — not the loader — opened this stream, so the builder is
        // responsible for closing it. A format package's path-based sink factory does exactly
        // this internally; here we spell it out.
        var stream = File.Create(path);

        // .To(loader) terminates the pipeline into an IEtlPipelineSink.
        // .DisposingOwned(stream) wraps that sink so the factory-opened stream is disposed
        // after RunAsync finishes — whether it succeeds or throws. IAsyncDisposable is preferred
        // (FileStream is disposed asynchronously on net8.0), falling back to IDisposable.
        await EtlPipeline
            .Create()
            .From(Lines())
            .To(new FileLineLoader(stream))
            .DisposingOwned(stream)
            .RunAsync();

        // Proof the stream was closed: if it were still open, File.OpenRead would throw
        // IOException (the file would still be locked for writing).
        Console.WriteLine($"{ConsoleColors.Green}Wrote and closed:{ConsoleColors.Reset} {path}\n");
        foreach (var line in await File.ReadAllLinesAsync(path))
        {
            Console.WriteLine($"  {line}");
        }

        File.Delete(path);
        Console.WriteLine($"\n{ConsoleColors.Yellow}Pipeline completed; the file stream was disposed by DisposingOwned.{ConsoleColors.Reset}");
    }


    private static async IAsyncEnumerable<string> Lines()
    {
        string[] fruit = ["apple", "banana", "cherry", "date"];
        foreach (var item in fruit)
        {
            await Task.Delay(25);
            yield return item;
        }
    }
}
