using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Provides a report of the current item count in an ETL process.
/// </summary>
/// <remarks>
/// This class can be used as a base class for other progress reports and expanded
/// with additional information such as total count, count remaining, etc.
/// </remarks>
public record Report
{
    /// <summary>
    /// Constructs a new instance of the <see cref="Report"/> class with the specified current item count.
    /// </summary>
    /// <param name="currentItemCount">The number of items processed so far. Must be greater than or equal to 0.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="currentItemCount"/> is less than 0.</exception>
    public Report(int currentItemCount)
    {
        if (currentItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentItemCount), "Current item count cannot be less than 0.");
        }

        CurrentItemCount = currentItemCount;
    }



    /// <summary>
    /// The number of items that have been processed so far in the ETL process.
    /// </summary>
    public int CurrentItemCount { get; }
}
