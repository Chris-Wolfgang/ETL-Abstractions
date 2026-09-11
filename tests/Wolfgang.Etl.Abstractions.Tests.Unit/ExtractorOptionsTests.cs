namespace Wolfgang.Etl.Abstractions.Tests.Unit;

public class ExtractorOptionsTests
{
    [Fact]
    public void Constructor_when_nothing_is_set_applies_the_documented_defaults()
    {
        var sut = new ExtractorOptions();

        Assert.Equal(1_000, sut.ReportingInterval);
        Assert.Equal(int.MaxValue, sut.MaximumItemCount);
        Assert.Equal(0, sut.SkipItemCount);
        Assert.Same(DefaultItemErrorPolicy.Abort, sut.ErrorPolicy);
    }



    [Fact]
    public void ReportingInterval_when_set_below_1_throws_ArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ExtractorOptions { ReportingInterval = 0 });
        Assert.Equal("value", ex.ParamName);
    }



    [Fact]
    public void MaximumItemCount_when_set_below_1_throws_ArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ExtractorOptions { MaximumItemCount = 0 });
        Assert.Equal("value", ex.ParamName);
    }



    [Fact]
    public void SkipItemCount_when_set_below_0_throws_ArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ExtractorOptions { SkipItemCount = -1 });
        Assert.Equal("value", ex.ParamName);
    }



    [Fact]
    public void ErrorPolicy_when_set_to_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new ExtractorOptions { ErrorPolicy = null! });
        Assert.Equal("value", ex.ParamName);
    }



    [Fact]
    public void With_when_one_member_is_changed_keeps_the_defaults_for_the_rest()
    {
        var original = new ExtractorOptions();

        var changed = original with { MaximumItemCount = 7 };

        Assert.Equal(7, changed.MaximumItemCount);
        Assert.Equal(original.ReportingInterval, changed.ReportingInterval);
        Assert.Equal(original.SkipItemCount, changed.SkipItemCount);
        Assert.Same(original.ErrorPolicy, changed.ErrorPolicy);
    }
}
