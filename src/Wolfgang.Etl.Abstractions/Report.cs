using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Provides a report of the current count in an ETL process.
/// </summary>
/// <remarks>
/// This class can be used as a base class for other progress reports and expanded
/// with additional information such as total count, count remaining, etc.
/// </remarks>
public record Report
{
    /// <summary>
    /// Constructs a new instance of the <see cref="Report"/> class with the specified current count.
    /// </summary>
    /// <param name="currentCount"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Report(int currentCount)
    {
        if (currentCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentCount), "Current count cannot be less than 0.");
        }

        CurrentCount = currentCount;
    }



    /// <summary>
    /// The number of items that have been processed so far in the ETL process.
    /// </summary>
    public int CurrentCount { get; }

}