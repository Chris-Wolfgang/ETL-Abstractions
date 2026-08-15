using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Holds the shared default item-error policy delegate in a non-generic type.
/// The base stages (<see cref="ExtractorBase{TSource, TProgress}"/> and friends)
/// are generic, so a <c>static</c> field declared on them would allocate one
/// copy per closed <c>&lt;T&gt;</c>. Keeping the stateless default here shares a
/// single instance across every instantiation.
/// </summary>
internal static class DefaultItemErrorPolicy
{
    /// <summary>
    /// The default per-item error policy: abort the run on the first item error.
    /// </summary>
    public static readonly Func<ItemErrorContext, ItemErrorAction> Abort =
        static _ => ItemErrorAction.Abort;
}
