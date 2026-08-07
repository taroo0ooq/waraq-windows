// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// WRQ-WIN-002 Phase 3-QA — expanded path gate + host probe matrix (Stagecraft QA).

using Waraq.Windows.Core;
using Waraq.Windows.Engines;
using Waraq.Windows.Host;

namespace Waraq.Windows.Tests;

public class AppInfoTests
{
    [Fact]
    public void Version_IsPhase4Line()
    {
        Assert.Contains("phase4", AppInfo.Version, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Product_IsWaraqWindows()
    {
        Assert.Contains("Waraq", AppInfo.ProductName, StringComparison.OrdinalIgnoreCase);
    }
}

public class LocalMediaPathGateTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("relative.mp4", false)]
    [InlineData("https://example.com/a.mp4", false)]
    [InlineData("http://example.com/a.mp4", false)]
    [InlineData("HTTPS://EXAMPLE.COM/a.gif", false)]
    [InlineData("smb://server/share/a.mp4", false)]
    [InlineData(@"\\server\share\a.mp4", false)]
    [InlineData("//server/share/a.mp4", false)]
    [InlineData(@"C:\Wallpapers\loop.mp4", true)]
    [InlineData(@"D:\media\clip.gif", true)]
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
    public void Normalize_RejectsForwardSlashUnc()
    {
        Assert.Throws<InvalidOperationException>(() =>
            LocalMediaPathGate.NormalizeExistingLocalFile("//server/share/a.mp4"));
    }

    [Fact]
    public void Normalize_RejectsEmpty()
    {
        Assert.Throws<InvalidOperationException>(() =>
            LocalMediaPathGate.NormalizeExistingLocalFile(""));
        Assert.Throws<InvalidOperationException>(() =>
            LocalMediaPathGate.NormalizeExistingLocalFile(null));
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

    [Fact]
    public void Normalize_QuotedTempFile_Ok()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"waraq-002q-{Guid.NewGuid():N}.mp4");
        File.WriteAllBytes(tmp, new byte[] { 0x00, 0x00 });
        try
        {
            var full = LocalMediaPathGate.NormalizeExistingLocalFile($"\"{tmp}\"");
            Assert.True(File.Exists(full));
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    [Fact]
    public void EnsureWithinSizeLimit_Empty_Throws()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"waraq-002e-{Guid.NewGuid():N}.gif");
        File.WriteAllBytes(tmp, Array.Empty<byte>());
        try
        {
            Assert.Throws<InvalidDataException>(() =>
                LocalMediaPathGate.EnsureWithinSizeLimit(tmp, MediaKind.Gif));
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    [Fact]
    public void EnsureWithinSizeLimit_GifOverCap_Throws()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"waraq-002big-{Guid.NewGuid():N}.gif");
        try
        {
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.SetLength(LocalMediaPathGate.MaxGifBytes + 1);
            }

            var ex = Assert.Throws<InvalidOperationException>(() =>
                LocalMediaPathGate.EnsureWithinSizeLimit(tmp, MediaKind.Gif));
            Assert.Contains("too large", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(tmp))
            {
                File.Delete(tmp);
            }
        }
    }

    [Fact]
    public void EnsureWithinSizeLimit_VideoOverCap_Throws()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"waraq-002bigv-{Guid.NewGuid():N}.mp4");
        try
        {
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.SetLength(LocalMediaPathGate.MaxVideoBytes + 1);
            }

            var ex = Assert.Throws<InvalidOperationException>(() =>
                LocalMediaPathGate.EnsureWithinSizeLimit(tmp, MediaKind.Video));
            Assert.Contains("too large", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(tmp))
            {
                File.Delete(tmp);
            }
        }
    }

    [Fact]
    public void SizeConstants_MatchDocumentedSoftCaps()
    {
        Assert.Equal(64L * 1024 * 1024, LocalMediaPathGate.MaxGifBytes);
        Assert.Equal(4L * 1024 * 1024 * 1024, LocalMediaPathGate.MaxVideoBytes);
    }

    [Fact]
    public void Normalize_Directory_Rejected()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"waraq-002dir-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var ex = Record.Exception(() => LocalMediaPathGate.NormalizeExistingLocalFile(dir));
            Assert.NotNull(ex);
            Assert.True(
                ex is FileNotFoundException or InvalidOperationException,
                $"Unexpected: {ex.GetType().FullName}");
        }
        finally
        {
            Directory.Delete(dir);
        }
    }
}

public class MediaPathClassifierTests
{
    [Theory]
    [InlineData("a.mp4", MediaKind.Video)]
    [InlineData("CLIP.MP4", MediaKind.Video)]
    [InlineData("b.GIF", MediaKind.Gif)]
    [InlineData("anim.gif", MediaKind.Gif)]
    [InlineData("c.png", MediaKind.Image)]
    [InlineData("still.jpg", MediaKind.Image)]
    [InlineData("x.txt", MediaKind.Unknown)]
    [InlineData("", MediaKind.Unknown)]
    [InlineData(null, MediaKind.Unknown)]
    public void Classify(string? path, MediaKind kind)
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
        var result = new DesktopWallpaperHost().Probe();
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public void VirtualScreen_PositiveSize()
    {
        var (x, y, w, h) = DesktopWallpaperHost.GetVirtualScreenPixels();
        Assert.True(w > 0);
        Assert.True(h > 0);
        _ = x;
        _ = y;
    }

    [Fact]
    public void MultiMonitor_Note_VirtualScreenCoversPrimaryAtLeast()
    {
        // Phase 3 MVP: one surface spans virtual desktop (all monitors).
        var (_, _, w, h) = DesktopWallpaperHost.GetVirtualScreenPixels();
        Assert.True(w >= 800);
        Assert.True(h >= 600);
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
