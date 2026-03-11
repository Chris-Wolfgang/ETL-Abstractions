namespace Wolfgang.Etl.Abstractions.Tests.Unit.Models;

public class EtlProgressTests
{
    [Fact]
    public void Constructor_when_passed_a_valid_value_stores_the_value()
    {
        var sut = new EtlProgress(42);
        Assert.Equal(42, sut.CurrentCount);
    }



    [Fact]
    public void Constructor_when_passed_zero_stores_zero()
    {
        var sut = new EtlProgress(0);
        Assert.Equal(0, sut.CurrentCount);
    }



    [Fact]
    public void Constructor_when_passed_a_negative_value_throws_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EtlProgress(-1));
    }



    [Fact]
    public void Constructor_when_passed_a_negative_value_throws_with_correct_param_name()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new EtlProgress(-1));
        Assert.Equal("currentCount", ex.ParamName);
    }



    [Fact]
    public void CurrentCount_returns_the_value_passed_to_the_constructor()
    {
        var sut = new EtlProgress(99);
        Assert.Equal(99, sut.CurrentCount);
    }
}
