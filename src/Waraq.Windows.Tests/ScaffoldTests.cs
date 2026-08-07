// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.

using Waraq.Windows.Core;
using Waraq.Windows.Engines;
using Waraq.Windows.Host;

namespace Waraq.Windows.Tests;

public class AppInfoTests
{
    [Fact]
    public void Version_IsPhase3Line()
    {
        Assert.Contains("phase3", AppInfo.Version, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void Normalize_RejectsHttp()
    {
        Assert.Throws<InvalidOperationException>(() =>
            LocalMediaPathGate.NormalizeExistingLocalFile("https://example.com/a.mp4"));
    }

    [Fact]
    public void Normalize_RejectsUnc()
    {
        Assert.Throws<InvalidOperationException>(() =>
            LocalMediaPathGate.NormalizeExistingLocalFile(@"\\server\share\a.mp4"));
    }

    [Fact]
    public void Normalize_MissingLocal_ThrowsFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() =>
            LocalMediaPathGate.NormalizeExistingLocalFile(@"C:\this\path\does\not\exist-waraq-002.mp4"));
    }

    [Fact]
    public void Normalize_TempFile_Ok()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"waraq-002-{Guid.NewGuid():N}.gif");
        File.WriteAllBytes(tmp, new byte[] { 0x47, 0x49, 0x46 });
        try
        {
            var full = LocalMediaPathGate.NormalizeExistingLocalFile(tmp);
            Assert.True(File.Exists(full));
            LocalMediaPathGate.EnsureWithinSizeLimit(full, MediaKind.Gif);
        }
        finally
        {
            File.Delete(tmp);
        }
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

public class EnginePhase3Tests
{
    [Fact]
    public void Phase3_Playable_VideoAndGifOnly()
    {
        Assert.True(EngineCatalog.IsPhase3Playable(MediaKind.Video));
        Assert.True(EngineCatalog.IsPhase3Playable(MediaKind.Gif));
        Assert.False(EngineCatalog.IsPhase3Playable(MediaKind.Image));
        Assert.False(EngineCatalog.IsPhase3Playable(MediaKind.Unknown));
    }
}

public class HostTests
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
    public void VirtualScreen_PositiveSize()
    {
        var (_, _, w, h) = DesktopWallpaperHost.GetVirtualScreenPixels();
        Assert.True(w > 0);
        Assert.True(h > 0);
    }
}

public class SettingsShellTests
{
    [Fact]
    public void BasicPanes_ExcludeDiagnostics()
    {
        var basic = Shell.SettingsNavCatalog.Visible(false).Select(p => p.Id).ToList();
        Assert.DoesNotContain(Shell.SettingsPaneId.Diagnostics, basic);
        Assert.Equal(7, basic.Count);
    }

    [Fact]
    public void Advanced_IncludesDiagnostics()
    {
        Assert.Contains(Shell.SettingsPaneId.Diagnostics,
            Shell.SettingsNavCatalog.Visible(true).Select(p => p.Id));
    }
}
