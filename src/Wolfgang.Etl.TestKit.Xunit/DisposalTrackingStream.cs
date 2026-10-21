using System.IO;
using System.Text;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// An in-memory stream that records whether it has been disposed. The multi-stream contract tests
/// put one under every reader and writer they hand to the stage under test.
/// </summary>
internal sealed class DisposalTrackingStream : MemoryStream
{
    public DisposalTrackingStream()
    {
    }



    public DisposalTrackingStream(byte[] buffer)
        : base(buffer)
    {
    }



    public bool IsDisposed { get; private set; }



    // MemoryStream.ToArray still works after disposal, so the written text stays readable.
    public string Text => Encoding.UTF8.GetString(ToArray());



    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }
}
