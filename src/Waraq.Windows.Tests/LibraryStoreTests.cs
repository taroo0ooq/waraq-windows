// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002 Phase 4-QA — library import/list/remove/profile matrix (Stagecraft QA).

using Waraq.Windows.Core;
using Waraq.Windows.Host;

namespace Waraq.Windows.Tests;

public class LibraryStoreTests : IDisposable
{
    private readonly string _root;
    private readonly LibraryPaths _paths;

    public LibraryStoreTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "waraq-lib-tests-" + Guid.NewGuid().ToString("N"));
        _paths = new LibraryPaths(_root);
        _paths.EnsureDirectories();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }

    private static string WriteMinimalGif(string dir, string name = "sample.gif")
    {
        var path = Path.Combine(dir, name);
        File.WriteAllBytes(path, Convert.FromBase64String(
            "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"));
        return path;
    }

    private static string WriteTinyPng(string dir, string name = "still.png")
    {
        // 1x1 PNG
        var path = Path.Combine(dir, name);
        File.WriteAllBytes(path, Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="));
        return path;
    }

    [Fact]
    public void Import_And_Reload_SurvivesNewStoreInstance()
    {
        var gif = WriteMinimalGif(_root);

        var store = new WallpaperLibraryStore(_paths);
        var entry = store.Import(gif);
        Assert.False(string.IsNullOrWhiteSpace(entry.Id));
        Assert.True(File.Exists(store.ResolveAbsolute(entry.RelativePath)));
        Assert.Equal(1, store.Items.Count);
        Assert.Equal("Gif", entry.Kind);

        var store2 = new WallpaperLibraryStore(_paths);
        Assert.Equal(1, store2.Items.Count);
        Assert.Equal(entry.Id, store2.Items[0].Id);
        Assert.Equal(entry.DisplayName, store2.Items[0].DisplayName);
    }

    [Fact]
    public void Import_List_OrdersNewestFirst()
    {
        var g1 = WriteMinimalGif(_root, "a.gif");
        var g2 = WriteMinimalGif(_root, "b.gif");
        var store = new WallpaperLibraryStore(_paths);
        var e1 = store.Import(g1);
        Thread.Sleep(15);
        var e2 = store.Import(g2);
        Assert.Equal(2, store.Items.Count);
        Assert.Equal(e2.Id, store.Items[0].Id);
        Assert.Equal(e1.Id, store.Items[1].Id);
    }

    [Fact]
    public void Import_Image_Supported()
    {
        var png = WriteTinyPng(_root);
        var store = new WallpaperLibraryStore(_paths);
        var entry = store.Import(png);
        Assert.Equal("Image", entry.Kind);
        Assert.True(File.Exists(store.ResolveAbsolute(entry.RelativePath)));
    }

    [Fact]
    public void Import_Video_WhenFixturePresent()
    {
        // Prefer repo e2e fixture if present
        var fixture = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "e2e", "fixtures", "sample.mp4"));
        if (!File.Exists(fixture))
        {
            fixture = Path.Combine(_root, "tiny.mp4");
            // Minimal non-empty file — import classifies by extension; size gate allows small video
            File.WriteAllBytes(fixture, new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 });
        }

        var store = new WallpaperLibraryStore(_paths);
        var entry = store.Import(fixture);
        Assert.Equal("Video", entry.Kind);
        Assert.True(File.Exists(store.ResolveAbsolute(entry.RelativePath)));
        Assert.NotNull(store.Find(entry.Id));
    }

    [Fact]
    public void Import_RejectsUnc()
    {
        var store = new WallpaperLibraryStore(_paths);
        Assert.ThrowsAny<Exception>(() => store.Import(@"\\server\share\a.gif"));
    }

    [Fact]
    public void Import_RejectsUrl()
    {
        var store = new WallpaperLibraryStore(_paths);
        Assert.ThrowsAny<Exception>(() => store.Import("https://example.com/a.gif"));
    }

    [Fact]
    public void Import_RejectsUnsupportedExtension()
    {
        var txt = Path.Combine(_root, "notes.txt");
        File.WriteAllText(txt, "hello");
        var store = new WallpaperLibraryStore(_paths);
        Assert.ThrowsAny<Exception>(() => store.Import(txt));
    }

    [Fact]
    public void Import_SamePath_ReplacesEntry()
    {
        var gif = WriteMinimalGif(_root, "same.gif");
        var store = new WallpaperLibraryStore(_paths);
        var a = store.Import(gif);
        var b = store.Import(gif);
        Assert.Equal(a.Id, b.Id);
        Assert.Equal(1, store.Items.Count);
    }

    [Fact]
    public void ApplyFromLibrary_ResolveAbsolute_IsLocalDriveFile()
    {
        var gif = WriteMinimalGif(_root);
        var store = new WallpaperLibraryStore(_paths);
        var entry = store.Import(gif);
        var abs = store.ResolveAbsolute(entry.RelativePath);
        Assert.True(Path.IsPathRooted(abs));
        Assert.True(File.Exists(abs));
        // Gate accepts library-managed local copy
        var normalized = LocalMediaPathGate.NormalizeExistingLocalFile(abs);
        Assert.True(File.Exists(normalized));
        Assert.Equal(MediaKind.Gif, MediaPathClassifier.Classify(normalized));
    }

    [Fact]
    public void Profiles_Upsert_SurvivesReload()
    {
        var store = new DisplayProfileStore(_paths);
        store.Upsert(@"DISPLAY\TEST\1", "Test Monitor", "abc123", WallpaperFitModeDto.Fill);
        store.Upsert(@"DISPLAY\TEST\1", "Test Monitor", "abc123", WallpaperFitModeDto.Fit);

        var store2 = new DisplayProfileStore(_paths);
        var p = store2.Get(@"DISPLAY\TEST\1");
        Assert.NotNull(p);
        Assert.Equal("abc123", p!.WallpaperId);
        Assert.Equal(WallpaperFitModeDto.Fit, p.Fit);
        Assert.Equal(1, store2.Profiles.Count);
    }

    [Fact]
    public void Profiles_EmptyKey_Throws()
    {
        var store = new DisplayProfileStore(_paths);
        Assert.Throws<ArgumentException>(() =>
            store.Upsert("  ", "x", "id", WallpaperFitModeDto.Fill));
    }

    [Fact]
    public void Profiles_MultiDisplay_Independent()
    {
        var store = new DisplayProfileStore(_paths);
        store.Upsert(@"DISPLAY\A", "A", "wall-a", WallpaperFitModeDto.Fill);
        store.Upsert(@"DISPLAY\B", "B", "wall-b", WallpaperFitModeDto.Stretch);
        Assert.Equal(2, store.Profiles.Count);
        Assert.Equal("wall-a", store.Get(@"DISPLAY\A")!.WallpaperId);
        Assert.Equal("wall-b", store.Get(@"DISPLAY\B")!.WallpaperId);
        Assert.Equal(WallpaperFitModeDto.Stretch, store.Get(@"DISPLAY\B")!.Fit);
    }

    [Fact]
    public void DisplayEnumerator_ReturnsAtLeastOne()
    {
        var list = DisplayEnumerator.EnumerateActiveDisplays();
        Assert.NotEmpty(list);
        Assert.All(list, d => Assert.False(string.IsNullOrWhiteSpace(d.Key)));
    }

    [Fact]
    public void Remove_DeletesEntryAndFile()
    {
        var gif = WriteMinimalGif(_root, "del.gif");
        var store = new WallpaperLibraryStore(_paths);
        var entry = store.Import(gif);
        var abs = store.ResolveAbsolute(entry.RelativePath);
        Assert.True(File.Exists(abs));
        Assert.True(store.Remove(entry.Id));
        Assert.Empty(store.Items);
        Assert.False(File.Exists(abs));
        Assert.False(store.Remove(entry.Id));
    }

    [Fact]
    public void HostProbe_NoRegression_AfterLibraryLoad()
    {
        var gif = WriteMinimalGif(_root);
        _ = new WallpaperLibraryStore(_paths).Import(gif);
        var probe = new DesktopWallpaperHost().Probe();
        Assert.False(string.IsNullOrWhiteSpace(probe.Message));
        Assert.Equal("WorkerW (Progman)", new DesktopWallpaperHost().StrategyName);
    }
}
