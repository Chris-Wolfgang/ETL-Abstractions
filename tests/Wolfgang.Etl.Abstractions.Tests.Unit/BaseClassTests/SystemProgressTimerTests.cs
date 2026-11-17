using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Unit tests for <see cref="SystemProgressTimer"/> exercised through
/// <see cref="ExtractorBase{TSource,TProgress}"/>.
/// </summary>
/// <remarks>
/// <see cref="SystemProgressTimer"/> is <see langword="internal"/> and is tested
/// indirectly by overriding <c>CreateProgressTimer</c> to capture the instance.
/// </remarks>
public class SystemProgressTimerTests
{
    // ------------------------------------------------------------------
    // StopTimer
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="SystemProgressTimer.StopTimer"/> stops the timer
    /// from firing further callbacks after it has been called.
    /// </summary>
    [Fact]
    public void StopTimer_prevents_further_callbacks()
    {
        var callbackCount = 0;
        ArmableTimerCore? core = null;
        using var timer = new SystemProgressTimer(_ => callbackCount++, state: null, onTick => core = new ArmableTimerCore(onTick));

        timer.Start(50);
        core!.FireIfArmed();
        timer.StopTimer();
        core.FireIfArmed();
        core.FireIfArmed();

        // Only the tick before StopTimer reached the callback: StopTimer disarmed the core, so the
        // later period boundaries never fire (#736: deterministic, no wall-clock waits).
        Assert.Equal(1, callbackCount);
    }



    // ------------------------------------------------------------------
    // Disposed guard in callback
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that a queued tick that fires after <see cref="SystemProgressTimer.Dispose"/>
    /// does not invoke the callback or raise <see cref="IProgressTimer.Elapsed"/>.
    /// </summary>
    [Fact]
    public void Callback_does_not_fire_after_dispose()
    {
        var callbackCount = 0;
        var elapsedCount = 0;
        ArmableTimerCore? core = null;
        var timer = new SystemProgressTimer(_ => callbackCount++, state: null, onTick => core = new ArmableTimerCore(onTick));
        timer.Elapsed += () => elapsedCount++;

        timer.Start(20);
        core!.FireIfArmed();
        timer.Dispose();

        // A tick the thread pool had already queued when Dispose ran still reaches OnTick; the
        // disposed guard must swallow it.
        core.FireRegardless();

        Assert.Equal(1, callbackCount);
        Assert.Equal(1, elapsedCount);
    }



    /// <summary>
    /// Verifies that <see cref="SystemProgressTimer.Dispose"/> does not return while a tick is
    /// still inside the callback, so a report sent after Dispose is always the last one.
    /// </summary>
    [Fact]
    public async Task Dispose_when_a_tick_is_in_flight_waits_for_it_to_finish()
    {
        var blockingTick = new BlockingTick();
        ArmableTimerCore? core = null;
        var timer = new SystemProgressTimer
        (
            _ => blockingTick.OnTick(),
            state: null,
            onTick => core = new ArmableTimerCore(onTick)
        );
        timer.Start(20);

        var tick = Task.Run(() => core!.FireIfArmed());
        blockingTick.WaitUntilEntered();
        var dispose = Task.Run
        (
            () =>
            {
                timer.Dispose();
                blockingTick.Record("disposed");
            }
        );

        var first = await Task.WhenAny(dispose, Task.Delay(250));
        Assert.NotSame(dispose, first);

        blockingTick.Release();
        await tick;
        await dispose;

        Assert.Equal(new[] { "tick", "disposed" }, blockingTick.Events());
    }



    /// <summary>
    /// Verifies that a tick arriving while the previous one is still inside the callback returns
    /// at once without reporting, instead of queuing behind it on a thread-pool thread.
    /// </summary>
    [Fact]
    public async Task Tick_when_the_previous_tick_is_still_reporting_is_dropped()
    {
        var blockingTick = new BlockingTick();
        var callbackCount = 0;
        ArmableTimerCore? core = null;
        using var timer = new SystemProgressTimer
        (
            _ =>
            {
                Interlocked.Increment(ref callbackCount);
                blockingTick.OnTick();
            },
            state: null,
            onTick => core = new ArmableTimerCore(onTick)
        );
        timer.Start(20);

        var firstTick = Task.Run(() => core!.FireIfArmed());
        blockingTick.WaitUntilEntered();

        var overlappingTick = Task.Run(() => core!.FireIfArmed());
        var finishedFirst = await Task.WhenAny(overlappingTick, Task.Delay(TimeSpan.FromSeconds(10)));

        // Released before asserting: a failed assertion must not leave the first tick holding the gate.
        blockingTick.Release();
        await firstTick;

        Assert.Same(overlappingTick, finishedFirst);
        Assert.Equal(1, Volatile.Read(ref callbackCount));
    }



    /// <summary>
    /// A timer callback that signals when it is entered and then blocks until released,
    /// recording the order in which it and the test's own steps finish.
    /// </summary>
    /// <remarks>
    /// Signals with a monitor rather than wait handles, so there is nothing to dispose and the
    /// test's lambdas can hold it for as long as they run.
    /// </remarks>
    private sealed class BlockingTick
    {
        private readonly object _gate = new();
        private readonly List<string> _events = new();
        private bool _entered;
        private bool _released;



        public void OnTick()
        {
            lock (_gate)
            {
                _entered = true;
                Monitor.PulseAll(_gate);
                while (!_released)
                {
                    Monitor.Wait(_gate);
                }

                _events.Add("tick");
            }
        }



        // Polls rather than waits: a wait that is only sometimes needed would leave a line
        // that is only sometimes covered.
        public void WaitUntilEntered() => SpinWait.SpinUntil(HasEntered);



        private bool HasEntered()
        {
            lock (_gate)
            {
                return _entered;
            }
        }



        public void Release()
        {
            lock (_gate)
            {
                _released = true;
                Monitor.PulseAll(_gate);
            }
        }



        public void Record(string name)
        {
            lock (_gate)
            {
                _events.Add(name);
            }
        }



        public string[] Events()
        {
            lock (_gate)
            {
                return _events.ToArray();
            }
        }
    }



    /// <summary>
    /// Verifies that a tick may dispose its own timer (the gate is re-entrant).
    /// </summary>
    [Fact]
    public void Dispose_from_inside_a_tick_does_not_deadlock()
    {
        var callbackCount = 0;
        ArmableTimerCore? core = null;
        // The callback needs the timer it belongs to, which exists only after construction;
        // the one-element array is assigned once and filled in afterwards.
        var timerSlot = new SystemProgressTimer?[1];
        var timer = new SystemProgressTimer
        (
            _ =>
            {
                callbackCount++;
                timerSlot[0]!.Dispose();
            },
            state: null,
            onTick => core = new ArmableTimerCore(onTick)
        );
        timerSlot[0] = timer;
        timer.Start(20);

        core!.FireIfArmed();
        core.FireRegardless();

        Assert.Equal(1, callbackCount);
    }



    /// <summary>
    /// Verifies that an exception thrown by the tick callback does not escape the tick (on a real
    /// timer it would be unhandled on a thread-pool thread) and stops further ticks.
    /// </summary>
    [Fact]
    public void Tick_when_the_callback_throws_swallows_the_exception_and_stops_the_timer()
    {
        var callbackCount = 0;
        ArmableTimerCore? core = null;
        using var timer = new SystemProgressTimer
        (
            _ =>
            {
                callbackCount++;
                throw new InvalidOperationException("report failed");
            },
            state: null,
            onTick => core = new ArmableTimerCore(onTick)
        );
        timer.Start(20);

        core!.FireIfArmed();
        core.FireIfArmed();

        Assert.Equal(1, callbackCount);
    }



    /// <summary>
    /// Verifies that an exception thrown by an <see cref="IProgressTimer.Elapsed"/> subscriber
    /// is contained the same way.
    /// </summary>
    [Fact]
    public void Tick_when_an_Elapsed_subscriber_throws_swallows_the_exception_and_stops_the_timer()
    {
        var elapsedCount = 0;
        ArmableTimerCore? core = null;
        using var timer = new SystemProgressTimer(_ => { }, state: null, onTick => core = new ArmableTimerCore(onTick));
        timer.Elapsed += () =>
        {
            elapsedCount++;
            throw new InvalidOperationException("subscriber failed");
        };
        timer.Start(20);

        core!.FireIfArmed();
        core.FireIfArmed();

        Assert.Equal(1, elapsedCount);
    }



    // ------------------------------------------------------------------
    // Test double
    // ------------------------------------------------------------------

    /// <summary>
    /// A deterministic <see cref="ITimerCore"/>: it fires only when the test says so, and
    /// <see cref="FireIfArmed"/> models a real timer by firing only while armed by <c>Change</c>.
    /// </summary>
    private sealed class ArmableTimerCore : ITimerCore
    {
        private readonly TimerCallback _onTick;
        private bool _armed;

        public ArmableTimerCore(TimerCallback onTick) => _onTick = onTick;

        public void Change(int dueTime, int period) => _armed = dueTime != Timeout.Infinite;

        public void Dispose() => _armed = false;

        public void FireIfArmed()
        {
            if (_armed)
            {
                _onTick(null);
            }
        }

        // A callback already dispatched before the timer was stopped or disposed.
        public void FireRegardless() => _onTick(null);
    }
}
