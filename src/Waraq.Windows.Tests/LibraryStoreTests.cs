// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002 Phase 4 — library + profiles persistence tests.

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

    [Fact]
    public void Import_And_Reload_SurvivesNewStoreInstance()
    {
        var gif = Path.Combine(_root, "sample.gif");
        // minimal GIF89a 1x1
        File.WriteAllBytes(gif, Convert.FromBase64String(
            "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"));

        var store = new WallpaperLibraryStore(_paths);
        var entry = store.Import(gif);
        Assert.False(string.IsNullOrWhiteSpace(entry.Id));
        Assert.True(File.Exists(store.ResolveAbsolute(entry.RelativePath)));
        Assert.Equal(1, store.Items.Count);

        var store2 = new WallpaperLibraryStore(_paths);
        Assert.Equal(1, store2.Items.Count);
        Assert.Equal(entry.Id, store2.Items[0].Id);
        Assert.Equal(entry.DisplayName, store2.Items[0].DisplayName);
    }

    [Fact]
    public void Import_RejectsUnc()
    {
        var store = new WallpaperLibraryStore(_paths);
        Assert.ThrowsAny<Exception>(() => store.Import(@"\\server\share\a.gif"));
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
    public void DisplayEnumerator_ReturnsAtLeastOne()
    {
        var list = DisplayEnumerator.EnumerateActiveDisplays();
        Assert.NotEmpty(list);
        Assert.All(list, d => Assert.False(string.IsNullOrWhiteSpace(d.Key)));
    }

    [Fact]
    public void Remove_DeletesEntryAndFile()
    {
        var gif = Path.Combine(_root, "del.gif");
        File.WriteAllBytes(gif, Convert.FromBase64String(
            "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"));
        var store = new WallpaperLibraryStore(_paths);
        var entry = store.Import(gif);
        var abs = store.ResolveAbsolute(entry.RelativePath);
        Assert.True(File.Exists(abs));
        Assert.True(store.Remove(entry.Id));
        Assert.Empty(store.Items);
        Assert.False(File.Exists(abs));
    }
}
