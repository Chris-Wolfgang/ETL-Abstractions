using System;
using Microsoft.Extensions.Logging;

namespace Wolfgang.Etl.ErrorPolicies;

/// <summary>
/// Cached <see cref="LoggerMessage"/> delegates for the logging error policies, so a per-item log
/// call allocates nothing on the hot path.
/// </summary>
internal static class ItemErrorPolicyLog
{
    internal static readonly Action<ILogger, long, Exception?> ItemFailedAndSkipped =
        LoggerMessage.Define<long>
        (
            LogLevel.Warning,
            new EventId(1, nameof(ItemFailedAndSkipped)),
            "Item {ItemNumber} failed to process and was skipped."
        );
}
