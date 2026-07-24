namespace Wolfgang.Etl.Abstractions.Tests.Unit;

public class ReportTests
{
    [Fact]
    public void Constructor_when_passed_a_valid_value_stores_the_value()
    {
        var sut = new Report(42);
        Assert.Equal(42, sut.CurrentItemCount);
    }



    [Fact]
    public void Constructor_when_passed_zero_stores_zero()
    {
        var sut = new Report(0);
        Assert.Equal(0, sut.CurrentItemCount);
    }



    [Fact]
    public void Constructor_when_passed_a_negative_value_throws_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Report(-1));
    }



    [Fact]
    public void Constructor_when_passed_a_negative_value_throws_with_correct_param_name()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Report(-1));
        Assert.Equal("currentItemCount", ex.ParamName);
    }


    [Fact]
    public void Constructor_when_passed_a_negative_value_throws_with_explanatory_message()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Report(-1));
        Assert.StartsWith("Current item count cannot be less than 0.", ex.Message);
    }



    [Fact]
    public void CurrentItemCount_returns_the_value_passed_to_the_constructor()
    {
        var sut = new Report(99);
        Assert.Equal(99, sut.CurrentItemCount);
    }



    [Fact]
    public void Two_Report_instances_with_same_value_are_equal()
    {
        var a = new Report(5);
        var b = new Report(5);
        Assert.Equal(a, b);
    }



    [Fact]
    public void Two_Report_instances_with_different_values_are_not_equal()
    {
        var a = new Report(5);
        var b = new Report(10);
        Assert.NotEqual(a, b);
    }



    [Fact]
    public void New_report_has_default_timing_and_throughput_values()
    {
        var sut = new Report(10);

        Assert.Null(sut.StartedAt);
        Assert.Equal(TimeSpan.Zero, sut.Elapsed);
        Assert.Null(sut.TotalItemCount);
        Assert.Equal(0d, sut.ItemsPerSecond);
        Assert.Null(sut.PercentComplete);
        Assert.Null(sut.EstimatedRemaining);
    }



    [Fact]
    public void TotalItemCount_when_set_to_a_negative_value_throws_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Report(0) { TotalItemCount = -1 });
    }


    [Fact]
    public void TotalItemCount_when_set_to_a_negative_value_throws_with_param_name_and_message()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Report(0) { TotalItemCount = -1 });
        Assert.Equal("value", ex.ParamName);
        Assert.StartsWith("Total item count cannot be less than 0.", ex.Message);
    }



    [Fact]
    public void ItemsPerSecond_when_time_has_elapsed_is_count_divided_by_seconds()
    {
        var sut = new Report(10) { Elapsed = TimeSpan.FromSeconds(2) };

        Assert.Equal(5d, sut.ItemsPerSecond);
    }



    [Fact]
    public void ItemsPerSecond_when_no_time_has_elapsed_is_zero()
    {
        var sut = new Report(10) { Elapsed = TimeSpan.Zero };

        Assert.Equal(0d, sut.ItemsPerSecond);
    }



    [Fact]
    public void PercentComplete_when_total_is_known_is_the_fraction_done()
    {
        var sut = new Report(50) { TotalItemCount = 200 };

        Assert.Equal(25d, sut.PercentComplete);
    }



    [Fact]
    public void PercentComplete_when_count_exceeds_total_is_clamped_to_100()
    {
        var sut = new Report(150) { TotalItemCount = 100 };

        Assert.Equal(100d, sut.PercentComplete);
    }



    [Fact]
    public void PercentComplete_when_total_is_zero_is_100()
    {
        var sut = new Report(0) { TotalItemCount = 0 };

        Assert.Equal(100d, sut.PercentComplete);
    }



    [Fact]
    public void PercentComplete_when_total_is_unknown_is_null()
    {
        var sut = new Report(50);

        Assert.Null(sut.PercentComplete);
    }



    [Fact]
    public void EstimatedRemaining_when_total_and_throughput_are_known_projects_remaining_time()
    {
        // 50 of 100 done in 10s => 5 items/s => 50 remaining => 10s left.
        var sut = new Report(50) { TotalItemCount = 100, Elapsed = TimeSpan.FromSeconds(10) };

        Assert.Equal(TimeSpan.FromSeconds(10), sut.EstimatedRemaining);
    }



    [Fact]
    public void EstimatedRemaining_when_count_has_reached_total_is_zero()
    {
        var sut = new Report(100) { TotalItemCount = 100, Elapsed = TimeSpan.FromSeconds(10) };

        Assert.Equal(TimeSpan.Zero, sut.EstimatedRemaining);
    }



    [Fact]
    public void EstimatedRemaining_is_zero_when_count_reached_total_even_with_no_elapsed_time()
    {
        // remaining == 0 must short-circuit to Zero *before* the throughput check, so a
        // completed run with a zero rate still reports Zero rather than null. Distinguishes
        // the "remaining <= 0" boundary from "remaining < 0".
        var sut = new Report(100) { TotalItemCount = 100, Elapsed = TimeSpan.Zero };

        Assert.Equal(TimeSpan.Zero, sut.EstimatedRemaining);
    }


    [Fact]
    public void EstimatedRemaining_when_total_is_unknown_is_null()
    {
        var sut = new Report(50) { Elapsed = TimeSpan.FromSeconds(10) };

        Assert.Null(sut.EstimatedRemaining);
    }



    [Fact]
    public void EstimatedRemaining_when_throughput_is_zero_is_null()
    {
        var sut = new Report(50) { TotalItemCount = 100, Elapsed = TimeSpan.Zero };

        Assert.Null(sut.EstimatedRemaining);
    }



    [Fact]
    public void EstimatedRemaining_when_projection_overflows_is_clamped_to_max()
    {
        // 1 item in 30 days => a near-zero rate; projecting the remaining int.MaxValue
        // items would exceed TimeSpan.MaxValue, so the estimate is clamped rather than throwing.
        var sut = new Report(1) { TotalItemCount = int.MaxValue, Elapsed = TimeSpan.FromDays(30) };

        Assert.Equal(TimeSpan.MaxValue, sut.EstimatedRemaining);
    }



    [Fact]
    public void StartedAt_and_Elapsed_round_trip_through_init()
    {
        var started = DateTimeOffset.UtcNow;
        var sut = new Report(1) { StartedAt = started, Elapsed = TimeSpan.FromSeconds(3) };

        Assert.Equal(started, sut.StartedAt);
        Assert.Equal(TimeSpan.FromSeconds(3), sut.Elapsed);
    }
}
