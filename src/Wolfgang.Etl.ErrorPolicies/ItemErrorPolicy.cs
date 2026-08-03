using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.ErrorPolicies;

/// <summary>
/// Ready-made policies for an ETL stage's error handling. Each is a
/// <see cref="Func{T, TResult}"/> from an <see cref="ItemErrorContext"/> to an
/// <see cref="ItemErrorAction"/>, so it can be assigned directly to a stage's <c>ErrorPolicy</c>
/// property (on <c>ExtractorBase</c> / <c>LoaderBase</c> / <c>TransformerBase</c>):
/// <example><code>
/// var deadLetters = new List&lt;ItemErrorContext&gt;();
/// var extractor = new SomeExtractor&lt;Record&gt;(source)
/// {
///     ErrorPolicy = ItemErrorPolicy.SkipDeadLetterAndLog(deadLetters, logger)
/// };
/// </code></example>
/// The dead-letter overloads write to a caller-owned sink (a collection or a channel), so its size —
/// and therefore the memory a bad feed can consume — stays under the caller's control.
/// </summary>
public static class ItemErrorPolicy
{
    /// <summary>
    /// A policy that discards the failed item and continues with the next one. The stage increments
    /// its error-item count (<c>CurrentErrorItemCount</c>) so the skip is never silent.
    /// </summary>
    public static Func<ItemErrorContext, ItemErrorAction> Skip { get; } = _ => ItemErrorAction.Skip;



    /// <summary>
    /// A policy that re-throws the failure and stops the run. Equivalent to leaving a stage's
    /// <c>ErrorPolicy</c> unset (which defaults to fail-fast); provided for symmetry and explicitness.
    /// </summary>
    public static Func<ItemErrorContext, ItemErrorAction> Abort { get; } = _ => ItemErrorAction.Abort;



    /// <summary>
    /// Logs the failure as a warning through <paramref name="logger"/>, then discards the item and
    /// continues.
    /// </summary>
    /// <param name="logger">The logger the returned policy writes each failure to.</param>
    /// <returns>A policy that logs and returns <see cref="ItemErrorAction.Skip"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
    public static Func<ItemErrorContext, ItemErrorAction> SkipAndLog(ILogger logger)
    {
        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        return context =>
        {
            ItemErrorPolicyLog.ItemFailedAndSkipped(logger, context.ItemNumber, context.Exception);
            return ItemErrorAction.Skip;
        };
    }



    /// <summary>
    /// Records the failure in <paramref name="deadLetters"/> (a "dead-letter" queue the caller owns),
    /// then discards the item and continues.
    /// </summary>
    /// <param name="deadLetters">The caller-owned collection each failed item is added to.</param>
    /// <returns>A policy that dead-letters and returns <see cref="ItemErrorAction.Skip"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deadLetters"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// A single stage invokes this policy serially, so a plain <see cref="List{T}"/> is safe. If you
    /// share one collection across stages running concurrently, either supply a thread-safe collection
    /// or use the <see cref="SkipAndDeadLetter(ChannelWriter{ItemErrorContext})"/> overload — the policy
    /// adds to the collection without locking.
    /// </remarks>
    public static Func<ItemErrorContext, ItemErrorAction> SkipAndDeadLetter(ICollection<ItemErrorContext> deadLetters)
    {
        if (deadLetters is null)
        {
            throw new ArgumentNullException(nameof(deadLetters));
        }

        return context =>
        {
            deadLetters.Add(context);
            return ItemErrorAction.Skip;
        };
    }



    /// <summary>
    /// Writes the failure to <paramref name="deadLetters"/> (a caller-owned channel) with
    /// <see cref="ChannelWriter{T}.TryWrite(T)"/>, then discards the item and continues. Because the
    /// hook is synchronous the non-blocking <c>TryWrite</c> is used, so a bounded channel that is full
    /// drops the failure — size the channel, or use <see cref="BoundedChannelFullMode"/>, accordingly.
    /// </summary>
    /// <param name="deadLetters">The caller-owned channel each failed item is written to.</param>
    /// <returns>A policy that dead-letters and returns <see cref="ItemErrorAction.Skip"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deadLetters"/> is <see langword="null"/>.</exception>
    public static Func<ItemErrorContext, ItemErrorAction> SkipAndDeadLetter(ChannelWriter<ItemErrorContext> deadLetters)
    {
        if (deadLetters is null)
        {
            throw new ArgumentNullException(nameof(deadLetters));
        }

        return context =>
        {
            deadLetters.TryWrite(context);
            return ItemErrorAction.Skip;
        };
    }



    /// <summary>
    /// Records the failure in <paramref name="deadLetters"/> and logs it as a warning through
    /// <paramref name="logger"/>, then discards the item and continues.
    /// </summary>
    /// <param name="deadLetters">The caller-owned collection each failed item is added to.</param>
    /// <param name="logger">The logger the returned policy writes each failure to.</param>
    /// <returns>A policy that dead-letters, logs, and returns <see cref="ItemErrorAction.Skip"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="deadLetters"/> or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// A single stage invokes this policy serially, so a plain <see cref="List{T}"/> is safe. If you
    /// share one collection across stages running concurrently, either supply a thread-safe collection
    /// or use the <see cref="SkipDeadLetterAndLog(ChannelWriter{ItemErrorContext}, ILogger)"/> overload —
    /// the policy adds to the collection without locking.
    /// </remarks>
    public static Func<ItemErrorContext, ItemErrorAction> SkipDeadLetterAndLog
    (
        ICollection<ItemErrorContext> deadLetters,
        ILogger logger
    )
    {
        if (deadLetters is null)
        {
            throw new ArgumentNullException(nameof(deadLetters));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        return context =>
        {
            deadLetters.Add(context);
            ItemErrorPolicyLog.ItemFailedAndSkipped(logger, context.ItemNumber, context.Exception);
            return ItemErrorAction.Skip;
        };
    }



    /// <summary>
    /// Writes the failure to <paramref name="deadLetters"/> with
    /// <see cref="ChannelWriter{T}.TryWrite(T)"/> and logs it as a warning through
    /// <paramref name="logger"/>, then discards the item and continues. See
    /// <see cref="SkipAndDeadLetter(ChannelWriter{ItemErrorContext})"/> for the <c>TryWrite</c> caveat —
    /// but unlike that logger-less overload, when the write is dropped (a bounded channel that is full)
    /// this policy logs a distinct warning so the lost failure record is never silent.
    /// </summary>
    /// <param name="deadLetters">The caller-owned channel each failed item is written to.</param>
    /// <param name="logger">The logger the returned policy writes each failure to.</param>
    /// <returns>A policy that dead-letters, logs, and returns <see cref="ItemErrorAction.Skip"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="deadLetters"/> or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public static Func<ItemErrorContext, ItemErrorAction> SkipDeadLetterAndLog
    (
        ChannelWriter<ItemErrorContext> deadLetters,
        ILogger logger
    )
    {
        if (deadLetters is null)
        {
            throw new ArgumentNullException(nameof(deadLetters));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        return context =>
        {
            if (deadLetters.TryWrite(context))
            {
                ItemErrorPolicyLog.ItemFailedAndSkipped(logger, context.ItemNumber, context.Exception);
            }
            else
            {
                ItemErrorPolicyLog.ItemDeadLetterDropped(logger, context.ItemNumber, context.Exception);
            }

            return ItemErrorAction.Skip;
        };
    }
}
