using System;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

/// <summary>
/// Sentinel exception thrown by test doubles when the pipeline routes a call to an
/// overload that should never be reached for that double's capability interface. Using a
/// dedicated type instead of a plain <see cref="InvalidOperationException"/> makes accidental
/// routing regressions easy to diagnose in test failure output.
/// </summary>
internal sealed class WrongOverloadCalledException : Exception
{
    public WrongOverloadCalledException(string overloadSignature)
        : base($"Unexpected call to {overloadSignature}. The pipeline routed through the wrong overload.")
    {
    }
}
