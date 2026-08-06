// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Waraq.Windows.Host;

namespace Waraq.Windows.Tests;

public class AppInfoTests
{
    [Fact]
    public void ProductName_IsStable()
    {
        Assert.Equal("Waraq for Windows", AppInfo.ProductName);
    }

    [Fact]
    public void ScaffoldStatus_MentionsWorkerWNotAttached()
    {
        Assert.Contains("WorkerW", AppInfo.ScaffoldStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("scaffold", AppInfo.ScaffoldStatus, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void License_IsGpl3()
    {
        Assert.Contains("GPL-3.0", AppInfo.License, StringComparison.Ordinal);
    }
}

public class DesktopWallpaperHostTests
{
    [Fact]
    public void StrategyName_IsWorkerW()
    {
        var host = new DesktopWallpaperHost();
        Assert.Equal("WorkerW (Progman)", host.StrategyName);
    }

    [Fact]
    public void Probe_DoesNotThrow_AndReportsProgmanState()
    {
        var host = new DesktopWallpaperHost();
        var result = host.Probe();

        Assert.False(string.IsNullOrWhiteSpace(result.Message));
        // On a normal interactive Windows desktop Progman should exist.
        // CI without interactive shell may differ — still must not throw.
        if (result.FoundProgman)
        {
            Assert.NotEqual(IntPtr.Zero, result.ProgmanHandle);
        }
    }
}
