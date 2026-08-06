// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Waraq.Windows.Engines;
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
    public void Version_IsPhase2Alpha()
    {
        Assert.Contains("0.2.0", AppInfo.Version, StringComparison.Ordinal);
    }

    [Fact]
    public void License_IsGpl3()
    {
        Assert.Contains("GPL-3.0", AppInfo.License, StringComparison.Ordinal);
    }
}

public class MediaPathClassifierTests
{
    [Theory]
    [InlineData("clip.mp4", MediaKind.Video)]
    [InlineData("CLIP.MP4", MediaKind.Video)]
    [InlineData(@"C:\wall\loop.webm", MediaKind.Video)]
    [InlineData("anim.gif", MediaKind.Gif)]
    [InlineData("photo.png", MediaKind.Unknown)]
    [InlineData("", MediaKind.Unknown)]
    [InlineData(null, MediaKind.Unknown)]
    public void Classify_ReturnsExpected(string? path, MediaKind expected)
    {
        Assert.Equal(expected, MediaPathClassifier.Classify(path));
    }

    [Fact]
    public void IsSupported_TrueForVideoAndGif()
    {
        Assert.True(MediaPathClassifier.IsSupported("a.mp4"));
        Assert.True(MediaPathClassifier.IsSupported("b.gif"));
        Assert.False(MediaPathClassifier.IsSupported("c.jpg"));
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
        if (result.FoundProgman)
        {
            Assert.NotEqual(IntPtr.Zero, result.ProgmanHandle);
        }
    }

    [Fact]
    public void GetVirtualScreenPixels_ReturnsPositiveSize()
    {
        var (_, _, w, h) = DesktopWallpaperHost.GetVirtualScreenPixels();
        Assert.True(w > 0);
        Assert.True(h > 0);
    }
}

public class WallpaperControllerTests
{
    [Fact]
    public void Apply_MissingFile_Throws()
    {
        using var controller = new WallpaperController();
        Assert.Throws<FileNotFoundException>(() =>
            controller.Apply(@"C:\this\path\does\not\exist-waraq-test.mp4"));
    }

    [Fact]
    public void Apply_UnsupportedExtension_Throws()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"waraq-test-{Guid.NewGuid():N}.png");
        File.WriteAllBytes(tmp, new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        try
        {
            using var controller = new WallpaperController();
            Assert.Throws<NotSupportedException>(() => controller.Apply(tmp));
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    [Fact]
    public void Stop_WhenIdle_DoesNotThrow()
    {
        using var controller = new WallpaperController();
        controller.Stop();
        Assert.False(controller.IsRunning);
    }
}
