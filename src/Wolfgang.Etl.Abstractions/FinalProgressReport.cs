using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Sends the final progress report of a run without letting it replace the run's own exception.
/// </summary>
internal static class FinalProgressReport
{
    /// <summary>
    /// Builds and delivers the final report.
    /// </summary>
    /// <typeparam name="TProgress">The progress report type.</typeparam>
    /// <param name="progress">The sink that receives the report.</param>
    /// <param name="createReport">Builds the report from the stage's current counters.</param>
    /// <param name="runFailed">
    /// <see langword="true"/> when the run is ending with an exception (a failure or a cancellation).
    /// The run's exception is the primary signal, so an exception thrown while building or delivering
    /// the report is then discarded; on a run that did not fail it propagates to the caller.
    /// </param>
    internal static void Send<TProgress>
    (
        IProgress<TProgress> progress,
        Func<TProgress> createReport,
        bool runFailed
    )
    {
        if (runFailed)
        {
            _ = TrySend(progress, createReport);
            return;
        }

        progress.Report(createReport());
    }



    // Returns whether the report was delivered. The caller above ignores the answer: on a failed
    // run the exception already unwinding the run is the one the caller of the stage needs.
    private static bool TrySend<TProgress>(IProgress<TProgress> progress, Func<TProgress> createReport)
    {
        try
        {
            progress.Report(createReport());
            return true;
        }
#pragma warning disable CA1031 // Do not catch general exception types — intentional: the run's own exception must reach the caller.
        catch (Exception)
#pragma warning restore CA1031
        {
            return false;
        }
    }
}
