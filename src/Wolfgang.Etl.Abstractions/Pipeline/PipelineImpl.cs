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


    /// <inheritdoc/>
    public string? Name { get; private set; }


    /// <inheritdoc/>
    public IPipeline WithName(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        return this;
    }


    /// <inheritdoc/>
    public Task RunAsync()
    {
        return RunAsync(CancellationToken.None);
    }


    /// <inheritdoc/>
    public Task RunAsync(CancellationToken token)
    {
        try
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
#pragma warning disable CA1031 // Do not catch general exception types — intentional: we forward every failure through the Task contract.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            return Task.FromException(ex);
        }
    }
}
