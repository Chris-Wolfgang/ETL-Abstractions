using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.ErrorPolicies.Tests.Unit;

public sealed class ItemErrorPolicyTests
{
    private static ItemErrorContext Context() =>
        new(42, new InvalidOperationException("boom"), () => "raw");



    [Fact]
    public void Skip_returns_Skip()
    {
        Assert.Equal(ItemErrorAction.Skip, ItemErrorPolicy.Skip(Context()));
    }



    [Fact]
    public void Abort_returns_Abort()
    {
        Assert.Equal(ItemErrorAction.Abort, ItemErrorPolicy.Abort(Context()));
    }



    [Fact]
    public void SkipAndLog_when_logger_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ItemErrorPolicy.SkipAndLog(null!));
    }



    [Fact]
    public void SkipAndLog_logs_the_failure_and_returns_Skip()
    {
        var logger = new RecordingLogger();

        var action = ItemErrorPolicy.SkipAndLog(logger)(Context());

        Assert.Equal(ItemErrorAction.Skip, action);
        Assert.Equal(1, logger.WarningCount);
    }



    [Fact]
    public void SkipAndDeadLetter_collection_when_deadLetters_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ItemErrorPolicy.SkipAndDeadLetter((ICollection<ItemErrorContext>)null!));
    }



    [Fact]
    public void SkipAndDeadLetter_collection_records_the_failure_and_returns_Skip()
    {
        var deadLetters = new List<ItemErrorContext>();
        var context = Context();

        var action = ItemErrorPolicy.SkipAndDeadLetter(deadLetters)(context);

        Assert.Equal(ItemErrorAction.Skip, action);
        Assert.Same(context, Assert.Single(deadLetters));
    }



    [Fact]
    public void SkipAndDeadLetter_channel_when_deadLetters_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ItemErrorPolicy.SkipAndDeadLetter((ChannelWriter<ItemErrorContext>)null!));
    }



    [Fact]
    public void SkipAndDeadLetter_channel_writes_the_failure_and_returns_Skip()
    {
        var channel = Channel.CreateUnbounded<ItemErrorContext>();
        var context = Context();

        var action = ItemErrorPolicy.SkipAndDeadLetter(channel.Writer)(context);

        Assert.Equal(ItemErrorAction.Skip, action);
        Assert.True(channel.Reader.TryRead(out var written));
        Assert.Same(context, written);
    }



    [Fact]
    public void SkipDeadLetterAndLog_collection_when_deadLetters_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ItemErrorPolicy.SkipDeadLetterAndLog((ICollection<ItemErrorContext>)null!, new RecordingLogger()));
    }



    [Fact]
    public void SkipDeadLetterAndLog_collection_when_logger_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ItemErrorPolicy.SkipDeadLetterAndLog(new List<ItemErrorContext>(), null!));
    }



    [Fact]
    public void SkipDeadLetterAndLog_collection_records_and_logs_and_returns_Skip()
    {
        var deadLetters = new List<ItemErrorContext>();
        var logger = new RecordingLogger();
        var context = Context();

        var action = ItemErrorPolicy.SkipDeadLetterAndLog(deadLetters, logger)(context);

        Assert.Equal(ItemErrorAction.Skip, action);
        Assert.Same(context, Assert.Single(deadLetters));
        Assert.Equal(1, logger.WarningCount);
    }



    [Fact]
    public void SkipDeadLetterAndLog_channel_when_deadLetters_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ItemErrorPolicy.SkipDeadLetterAndLog((ChannelWriter<ItemErrorContext>)null!, new RecordingLogger()));
    }



    [Fact]
    public void SkipDeadLetterAndLog_channel_when_logger_is_null_throws_ArgumentNullException()
    {
        var channel = Channel.CreateUnbounded<ItemErrorContext>();
        Assert.Throws<ArgumentNullException>(() => ItemErrorPolicy.SkipDeadLetterAndLog(channel.Writer, null!));
    }



    [Fact]
    public void SkipDeadLetterAndLog_channel_writes_and_logs_and_returns_Skip()
    {
        var channel = Channel.CreateUnbounded<ItemErrorContext>();
        var logger = new RecordingLogger();
        var context = Context();

        var action = ItemErrorPolicy.SkipDeadLetterAndLog(channel.Writer, logger)(context);

        Assert.Equal(ItemErrorAction.Skip, action);
        Assert.True(channel.Reader.TryRead(out var written));
        Assert.Same(context, written);
        Assert.Equal(1, logger.WarningCount);
        Assert.Equal(1, logger.LastEventId.Id);            // ItemFailedAndSkipped
    }



    [Fact]
    public void SkipDeadLetterAndLog_channel_when_full_logs_the_dropped_write_and_returns_Skip()
    {
        // A bounded channel at capacity: TryWrite returns false, so the failure record is dropped.
        var channel = Channel.CreateBounded<ItemErrorContext>
        (
            new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.Wait }
        );
        var prefilled = Context();
        Assert.True(channel.Writer.TryWrite(prefilled));   // fill it to capacity

        var logger = new RecordingLogger();
        var context = Context();

        var action = ItemErrorPolicy.SkipDeadLetterAndLog(channel.Writer, logger)(context);

        Assert.Equal(ItemErrorAction.Skip, action);        // still skips — a full sink never aborts the run
        Assert.True(channel.Reader.TryRead(out var only));
        Assert.Same(prefilled, only);                      // the new failure was dropped, not enqueued
        Assert.False(channel.Reader.TryRead(out _));       // nothing else in the channel
        Assert.Equal(1, logger.WarningCount);              // the drop is logged, not silent
        Assert.Equal(2, logger.LastEventId.Id);            // ItemDeadLetterDropped
    }



    private sealed class RecordingLogger : ILogger
    {
        public int WarningCount { get; private set; }

        public EventId LastEventId { get; private set; }

        // ILogger-required member; the policies under test log warnings but never open a scope, so this is never called.
        [ExcludeFromCodeCoverage]
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>
        (
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            if (logLevel == LogLevel.Warning)
            {
                WarningCount++;
                LastEventId = eventId;
            }
        }



        // Only ever returned by the never-called BeginScope above, so its members are unreachable in tests.
        [ExcludeFromCodeCoverage]
        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
