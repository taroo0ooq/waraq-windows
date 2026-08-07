// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.

using Waraq.Windows.Core;
using Waraq.Windows.Engines;
using Waraq.Windows.Host;
using Waraq.Windows.Shell;

namespace Waraq.Windows.Tests;

public class AppInfoTests
{
    [Fact]
    public void Version_IsPhase1()
    {
        Assert.Contains("phase1", AppInfo.Version, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Placeholder_MentionsScaffold()
    {
        Assert.Contains("scaffold", AppInfo.PlaceholderTitle, StringComparison.OrdinalIgnoreCase);
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
        var ok = LocalMediaPathGate.IsAllowed(path, out _);
        Assert.Equal(expected, ok);
    }

    [Fact]
    public void EnsureAllowed_ThrowsOnUrl()
    {
        Assert.Throws<InvalidOperationException>(() =>
            LocalMediaPathGate.EnsureAllowed("http://evil/x.gif"));
    }
}

public class MediaPathClassifierTests
{
    [Theory]
    [InlineData("a.mp4", MediaKind.Video)]
    [InlineData("b.GIF", MediaKind.Gif)]
    [InlineData("c.png", MediaKind.Image)]
    [InlineData("d.txt", MediaKind.Unknown)]
    public void Classify(string path, MediaKind kind)
    {
        Assert.Equal(kind, MediaPathClassifier.Classify(path));
    }
}

public class HostAndShellTests
{
    [Fact]
    public void HostStrategy_IsWorkerW()
    {
        Assert.Equal("WorkerW (Progman)", new DesktopWallpaperHost().StrategyName);
    }

    [Fact]
    public void Probe_DoesNotThrow()
    {
        var r = new DesktopWallpaperHost().Probe();
        Assert.False(string.IsNullOrWhiteSpace(r.Message));
    }

    [Fact]
    public void Shell_HasBasicPanes()
    {
        Assert.Contains("General", SettingsNavCatalog.BasicPanes);
        Assert.Contains("Gallery", SettingsNavCatalog.BasicPanes);
        Assert.Equal(7, SettingsNavCatalog.BasicPanes.Count);
    }

    [Fact]
    public void EngineCatalog_ListsProceduralSet()
    {
        Assert.Contains(EngineCatalog.PlannedEngines, e => e.Contains("Aurora", StringComparison.Ordinal));
        Assert.True(EngineCatalog.PlannedEngines.Count >= 6);
    }
}
