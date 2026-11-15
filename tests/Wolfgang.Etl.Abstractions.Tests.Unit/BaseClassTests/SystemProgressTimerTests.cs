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
        using var tickEntered = new ManualResetEventSlim();
        using var releaseTick = new ManualResetEventSlim();
        var events = new List<string>();
        ArmableTimerCore? core = null;
        var timer = new SystemProgressTimer
        (
            _ =>
            {
                tickEntered.Set();
                releaseTick.Wait();
                lock (events)
                {
                    events.Add("tick");
                }
            },
            state: null,
            onTick => core = new ArmableTimerCore(onTick)
        );
        timer.Start(20);

        var tick = Task.Run(() => core!.FireIfArmed());
        tickEntered.Wait();
        var dispose = Task.Run
        (
            () =>
            {
                timer.Dispose();
                lock (events)
                {
                    events.Add("disposed");
                }
            }
        );

        var first = await Task.WhenAny(dispose, Task.Delay(250));
        Assert.NotSame(dispose, first);

        releaseTick.Set();
        await tick;
        await dispose;

        Assert.Equal(new[] { "tick", "disposed" }, events);
    }



    /// <summary>
    /// Verifies that a tick may dispose its own timer (the gate is re-entrant).
    /// </summary>
    [Fact]
    public void Dispose_from_inside_a_tick_does_not_deadlock()
    {
        var callbackCount = 0;
        ArmableTimerCore? core = null;
        SystemProgressTimer? timer = null;
        timer = new SystemProgressTimer
        (
            _ =>
            {
                callbackCount++;
                timer!.Dispose();
            },
            state: null,
            onTick => core = new ArmableTimerCore(onTick)
        );
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
