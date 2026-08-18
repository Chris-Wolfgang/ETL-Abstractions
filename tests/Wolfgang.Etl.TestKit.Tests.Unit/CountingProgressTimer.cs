using System;
using System.Diagnostics.CodeAnalysis;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// A test <see cref="IProgressTimer"/> that fires <see cref="Elapsed"/> on demand via
/// <see cref="Fire"/> and records how often <see cref="StopTimer"/> and <see cref="Dispose"/>
/// are called. Unlike <c>ManualProgressTimer</c>, <see cref="Dispose"/> does <b>not</b> clear
/// the subscriber list, so a single injected instance can survive the base class's
/// per-run <c>timer.Dispose()</c> and still be inspected across multiple runs — which is
/// exactly what the duplicate-subscription guard tests need.
/// </summary>
internal sealed class CountingProgressTimer : IProgressTimer
{
    public event Action? Elapsed;



    /// <summary>The number of delegates currently subscribed to <see cref="Elapsed"/>.</summary>
    public int SubscriberCount => Elapsed?.GetInvocationList().Length ?? 0;



    /// <summary>How many times <see cref="StopTimer"/> has been called.</summary>
    public int StopTimerCallCount { get; private set; }



    /// <summary>How many times <see cref="Dispose"/> has been called.</summary>
    public int DisposeCallCount { get; private set; }



    /// <summary>Raises <see cref="Elapsed"/> synchronously on the calling thread.</summary>
    public void Fire() => Elapsed?.Invoke();



    [ExcludeFromCodeCoverage]
    public void Start(int intervalMilliseconds)
    {
    }



    public void StopTimer() => StopTimerCallCount++;



    // Deliberately does not clear Elapsed: the caller owns the injected timer, and the
    // subscriber list must remain observable after the base class disposes it per run.
    public void Dispose() => DisposeCallCount++;
}
