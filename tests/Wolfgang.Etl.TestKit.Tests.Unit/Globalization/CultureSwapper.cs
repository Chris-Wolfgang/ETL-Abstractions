using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Wolfgang.Etl.TestKit.Tests.Unit.Globalization;

/// <summary>
/// Swaps both <see cref="CultureInfo.CurrentCulture"/> and
/// <see cref="CultureInfo.CurrentUICulture"/> to a hostile culture for the
/// duration of a test and restores the originals on <see cref="Dispose"/>.
/// </summary>
/// <remarks>
/// Since .NET Framework 4.6, <see cref="CultureInfo.CurrentCulture"/> flows
/// across <c>await</c> continuations via the execution context, so a value set
/// before an asynchronous call remains in effect inside it. Wrap each test body
/// in a <c>using</c> so the ambient culture is always restored, even on failure.
/// </remarks>
[ExcludeFromCodeCoverage]
internal sealed class CultureSwapper : IDisposable
{
    private readonly CultureInfo _originalCulture;

    private readonly CultureInfo _originalUiCulture;



    public CultureSwapper(string cultureName)
    {
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUiCulture = CultureInfo.CurrentUICulture;

        var culture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }



    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUiCulture;
    }
}
