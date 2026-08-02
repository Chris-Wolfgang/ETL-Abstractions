using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.ErrorPolicies;

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
    }



    private sealed class RecordingLogger : ILogger
    {
        public int WarningCount { get; private set; }

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
            }
        }



        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
