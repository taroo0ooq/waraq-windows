// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002-HF1-D1 — host attach fail-closed tests.

using Waraq.Windows.Host;

namespace Waraq.Windows.Tests;

public class HostAttachFailClosedTests
{
    [Fact]
    public void TryAttachHwnd_ZeroHandle_FailsClosed()
    {
        var host = new DesktopWallpaperHost();
        var r = host.TryAttachHwnd(IntPtr.Zero, 0, 0, 100, 100);
        Assert.False(r.Success);
        Assert.Contains("Invalid", r.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AttachHwnd_ZeroHandle_Throws()
    {
        var host = new DesktopWallpaperHost();
        Assert.Throws<InvalidOperationException>(() => host.AttachHwnd(IntPtr.Zero, 0, 0, 10, 10));
    }

    [Fact]
    public void Probe_DoesNotThrow()
    {
        var host = new DesktopWallpaperHost();
        var p = host.Probe();
        Assert.False(string.IsNullOrWhiteSpace(p.Message));
        // Strategy name stable for diagnostics
        Assert.Contains("WorkerW", host.StrategyName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void VirtualScreen_PositiveSize()
    {
        var (x, y, w, h) = DesktopWallpaperHost.GetVirtualScreenPixels();
        _ = (x, y);
        Assert.True(w > 0);
        Assert.True(h > 0);
    }

    [Fact]
    public void WallpaperAttachResult_FailFactory()
    {
        var f = WallpaperAttachResult.Fail("nope");
        Assert.False(f.Success);
        Assert.Equal("nope", f.Message);
        Assert.Equal(IntPtr.Zero, f.WorkerW);
    }
}
