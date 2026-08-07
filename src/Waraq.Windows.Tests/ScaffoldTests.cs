// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.

using Waraq.Windows.Core;
using Waraq.Windows.Engines;
using Waraq.Windows.Host;
using Waraq.Windows.Shell;

namespace Waraq.Windows.Tests;

public class AppInfoTests
{
    [Fact]
    public void Version_IsPhase2Line()
    {
        Assert.Contains("phase2", AppInfo.Version, StringComparison.OrdinalIgnoreCase);
    }
}

public class LocalMediaPathGateTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("relative.mp4", false)]
    [InlineData("https://example.com/a.mp4", false)]
    [InlineData(@"\\server\share\a.mp4", false)]
    [InlineData(@"C:\Wallpapers\loop.mp4", true)]
    public void IsAllowed_Expected(string? path, bool expected)
    {
        Assert.Equal(expected, LocalMediaPathGate.IsAllowed(path, out _));
    }
}

public class MediaPathClassifierTests
{
    [Theory]
    [InlineData("a.mp4", MediaKind.Video)]
    [InlineData("b.GIF", MediaKind.Gif)]
    [InlineData("c.png", MediaKind.Image)]
    public void Classify(string path, MediaKind kind)
    {
        Assert.Equal(kind, MediaPathClassifier.Classify(path));
    }
}

public class SettingsShellTests
{
    [Fact]
    public void BasicPanes_ExcludeDiagnostics()
    {
        var basic = SettingsNavCatalog.Visible(advanced: false).Select(p => p.Id).ToList();
        Assert.DoesNotContain(SettingsPaneId.Diagnostics, basic);
        Assert.Contains(SettingsPaneId.General, basic);
        Assert.Contains(SettingsPaneId.Gallery, basic);
        Assert.Equal(7, basic.Count);
    }

    [Fact]
    public void AdvancedPanes_IncludeDiagnostics()
    {
        var adv = SettingsNavCatalog.Visible(advanced: true).Select(p => p.Id).ToList();
        Assert.Contains(SettingsPaneId.Diagnostics, adv);
        Assert.Equal(8, adv.Count);
    }

    [Fact]
    public void EachPane_HasMacScreenshotRef()
    {
        Assert.All(SettingsNavCatalog.All, p => Assert.False(string.IsNullOrWhiteSpace(p.MacScreenshotRef)));
    }

    [Fact]
    public void ViewModel_DefaultTitle_IsSettings()
    {
        Assert.Equal("Settings", new SettingsShellViewModel().WindowTitle);
    }
}

public class HostAndEngineTests
{
    [Fact]
    public void HostStrategy_IsWorkerW()
    {
        Assert.Equal("WorkerW (Progman)", new DesktopWallpaperHost().StrategyName);
    }

    [Fact]
    public void Probe_DoesNotThrow()
    {
        Assert.False(string.IsNullOrWhiteSpace(new DesktopWallpaperHost().Probe().Message));
    }

    [Fact]
    public void EngineCatalog_HasProcedural()
    {
        Assert.Contains(EngineCatalog.PlannedEngines, e => e.Contains("Aurora", StringComparison.Ordinal));
    }
}
