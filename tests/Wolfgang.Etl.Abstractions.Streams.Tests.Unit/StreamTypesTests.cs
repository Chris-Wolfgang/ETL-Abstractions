using System;
using System.IO;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Streams.Tests.Unit;

public class StreamTypesTests
{
    [Fact]
    public void NamedStreamSource_keeps_its_reader_and_name()
    {
        using var reader = new StreamReader(new MemoryStream());

        var sut = new NamedStreamSource(reader, "in.csv");

        Assert.Same(reader, sut.Reader);
        Assert.Equal("in.csv", sut.Name);
    }



    [Fact]
    public void NamedStreamSource_name_defaults_to_null()
    {
        using var reader = new StreamReader(new MemoryStream());

        Assert.Null(new NamedStreamSource(reader).Name);
    }



    [Fact]
    public void NamedStreamSource_when_reader_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new NamedStreamSource(null!));

        Assert.Equal("reader", ex.ParamName);
    }



    [Fact]
    public void NamedStreamDestination_keeps_its_writer_and_name()
    {
        using var writer = new StreamWriter(new MemoryStream());

        var sut = new NamedStreamDestination(writer, "out.csv");

        Assert.Same(writer, sut.Writer);
        Assert.Equal("out.csv", sut.Name);
    }



    [Fact]
    public void NamedStreamDestination_name_defaults_to_null()
    {
        using var writer = new StreamWriter(new MemoryStream());

        Assert.Null(new NamedStreamDestination(writer).Name);
    }



    [Fact]
    public void NamedStreamDestination_when_writer_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new NamedStreamDestination(null!));

        Assert.Equal("writer", ex.ParamName);
    }



    [Fact]
    public void StreamSourceError_keeps_its_values()
    {
        var exception = new InvalidDataException("bad");

        var sut = new StreamSourceError("in.csv", 2, exception);

        Assert.Equal("in.csv", sut.Name);
        Assert.Equal(2, sut.SourceIndex);
        Assert.Same(exception, sut.Exception);
    }



    [Fact]
    public void StreamSourceError_when_exception_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new StreamSourceError(null, 0, null!));

        Assert.Equal("exception", ex.ParamName);
    }



    [Fact]
    public void StreamSourceError_when_sourceIndex_is_negative_throws_ArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new StreamSourceError(null, -1, new InvalidDataException()));

        Assert.Equal("sourceIndex", ex.ParamName);
    }



    [Fact]
    public void MultiStreamProgress_keeps_its_values_and_the_Report_values()
    {
        var started = new DateTimeOffset(2026, 10, 21, 0, 0, 0, TimeSpan.Zero);

        var sut = new MultiStreamProgress(7, started, TimeSpan.FromSeconds(2), 2, 3, "b.csv", 1);

        Assert.Equal(7, sut.CurrentItemCount);
        Assert.Equal(started, sut.StartedAt);
        Assert.Equal(TimeSpan.FromSeconds(2), sut.Elapsed);
        Assert.Equal(2, sut.CurrentSourceNumber);
        Assert.Equal(3, sut.TotalSourceCount);
        Assert.Equal("b.csv", sut.CurrentSourceName);
        Assert.Equal(1, sut.CurrentFailedSourceCount);
    }



    [Fact]
    public void MultiStreamProgress_has_value_equality()
    {
        var a = new MultiStreamProgress(1, null, TimeSpan.Zero, 1, -1, null, 0);
        var b = new MultiStreamProgress(1, null, TimeSpan.Zero, 1, -1, null, 0);
        var c = new MultiStreamProgress(1, null, TimeSpan.Zero, 2, -1, null, 0);

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }



    [Theory]
    [InlineData(-1, 0, 0, "currentSourceNumber")]
    [InlineData(0, -2, 0, "totalSourceCount")]
    [InlineData(0, 0, -1, "currentFailedSourceCount")]
    public void MultiStreamProgress_when_a_count_is_out_of_range_throws_ArgumentOutOfRangeException
    (
        int currentSourceNumber,
        int totalSourceCount,
        int currentFailedSourceCount,
        string expectedParameter
    )
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new MultiStreamProgress(0, null, TimeSpan.Zero, currentSourceNumber, totalSourceCount, null, currentFailedSourceCount)
        );

        Assert.Equal(expectedParameter, ex.ParamName);
    }
}
