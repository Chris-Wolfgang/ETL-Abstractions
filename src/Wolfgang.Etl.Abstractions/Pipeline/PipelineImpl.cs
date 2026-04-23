using System;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IPipeline"/> implementation. Captures a single run delegate and enforces
/// one-shot execution.
/// </summary>
internal sealed class PipelineImpl : IPipeline
{
    private readonly Func<CancellationToken, Task> _run;
    private int _runCount;


    internal PipelineImpl(Func<CancellationToken, Task> run)
    {
        _run = run;
    }


    public string? Name { get; private set; }


    public IPipeline WithName(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        return this;
    }


    public Task RunAsync()
    {
        return RunAsync(CancellationToken.None);
    }


    public Task RunAsync(CancellationToken token)
    {
        if (Interlocked.Exchange(ref _runCount, 1) != 0)
        {
            throw new InvalidOperationException
            (
                "Pipeline has already been run. Construct a new pipeline for each run."
            );
        }

        return _run(token);
    }
}
