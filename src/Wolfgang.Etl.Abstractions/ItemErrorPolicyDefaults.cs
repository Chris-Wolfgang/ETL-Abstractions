using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Shared default item-error policy delegate. Hosted on a non-generic type so all closed
/// generic stage bases (<see cref="ExtractorBase{TSource, TProgress}"/>,
/// <see cref="LoaderBase{TDestination, TProgress}"/>,
/// <see cref="TransformerBase{TSource, TDestination, TProgress}"/>) share the same instance
/// instead of paying an allocation per closed generic type.
/// </summary>
internal static class ItemErrorPolicyDefaults
{
    public static readonly Func<ItemErrorContext, ItemErrorAction> Abort = static _ => ItemErrorAction.Abort;
}
