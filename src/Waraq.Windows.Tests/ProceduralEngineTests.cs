// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002 Phase 5-QA — procedural engines matrix (Stagecraft QA).

using Waraq.Windows.Core;
using Waraq.Windows.Engines;
using Waraq.Windows.Engines.Procedural;
using Waraq.Windows.Host;

namespace Waraq.Windows.Tests;

public class ProceduralEngineTests
{
    public static readonly string[] ExpectedIds =
    [
        "aurora",
        "matrix-rain",
        "synthwave",
        "starfield",
        "neural-network",
        "animated-gradient",
    ];

    [Fact]
    public void Catalog_HasSixEngines_MatchingMacSet()
    {
        Assert.Equal(6, ProceduralCatalog.All.Count);
        foreach (var id in ExpectedIds)
        {
            Assert.Contains(ProceduralCatalog.All, d => d.Id == id);
        }
    }

    [Fact]
    public void Catalog_Ids_AreUnique_AndNonEmptyNames()
    {
        var ids = ProceduralCatalog.All.Select(d => d.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.All(ProceduralCatalog.All, d =>
        {
            Assert.False(string.IsNullOrWhiteSpace(d.Id));
            Assert.False(string.IsNullOrWhiteSpace(d.DisplayName));
        });
    }

    [Theory]
    [InlineData("aurora")]
    [InlineData("matrix-rain")]
    [InlineData("synthwave")]
    [InlineData("starfield")]
    [InlineData("neural-network")]
    [InlineData("animated-gradient")]
    public void Create_ReturnsMatchingId(string id)
    {
        var engine = ProceduralCatalog.Create(id);
        Assert.Equal(id, engine.Id);
        Assert.False(string.IsNullOrWhiteSpace(engine.DisplayName));
    }

    [Theory]
    [InlineData("aurora")]
    [InlineData("matrix-rain")]
    [InlineData("synthwave")]
    [InlineData("starfield")]
    [InlineData("neural-network")]
    [InlineData("animated-gradient")]
    public void EachEngine_RendersFrame_WithoutThrow(string id)
    {
        var engine = ProceduralCatalog.Create(id);
        engine.Reset(7);
        var w = 64;
        var h = 36;
        var buf = new byte[w * h * 4];
        engine.RenderFrame(buf, w, h, 0.5);
        engine.RenderFrame(buf, w, h, 1.25);
        Assert.Contains(buf, b => b != 0);
    }

    [Theory]
    [InlineData("aurora")]
    [InlineData("matrix-rain")]
    [InlineData("synthwave")]
    [InlineData("starfield")]
    [InlineData("neural-network")]
    [InlineData("animated-gradient")]
    public void EachEngine_SecondFrame_CanDifferOrStayValid(string id)
    {
        var engine = ProceduralCatalog.Create(id);
        engine.Reset(3);
        const int w = 48;
        const int h = 27;
        var a = new byte[w * h * 4];
        var b = new byte[w * h * 4];
        engine.RenderFrame(a, w, h, 0.0);
        engine.RenderFrame(b, w, h, 2.5);
        Assert.Contains(a, x => x != 0);
        Assert.Contains(b, x => x != 0);
        // Most engines animate; if identical still OK as long as non-zero (static frames rare).
    }

    [Fact]
    public void UnknownEngine_Throws()
    {
        Assert.Throws<ArgumentException>(() => ProceduralCatalog.Create("nope"));
        Assert.Throws<ArgumentException>(() => ProceduralCatalog.Create(""));
    }

    [Fact]
    public void EngineCatalog_ListsProcedural()
    {
        Assert.Equal(6, EngineCatalog.ProceduralEngines.Count);
        Assert.Equal(ExpectedIds.OrderBy(x => x), EngineCatalog.ProceduralEngines.Select(d => d.Id).OrderBy(x => x));
        Assert.Contains(EngineCatalog.PlannedEngines, e => e.Contains("Aurora", StringComparison.Ordinal));
    }

    [Fact]
    public void HostProbe_NoRegression_WithProceduralCatalogLoaded()
    {
        _ = ProceduralCatalog.All.Count;
        _ = ProceduralCatalog.Create("aurora");
        var probe = new DesktopWallpaperHost().Probe();
        Assert.False(string.IsNullOrWhiteSpace(probe.Message));
        Assert.Equal("WorkerW (Progman)", new DesktopWallpaperHost().StrategyName);
    }

    [Fact]
    public void LibraryImport_NoRegression_AlongsideProceduralCatalog()
    {
        var root = Path.Combine(Path.GetTempPath(), "waraq-p5qa-" + Guid.NewGuid().ToString("N"));
        var paths = new LibraryPaths(root);
        paths.EnsureDirectories();
        try
        {
            var gif = Path.Combine(root, "s.gif");
            File.WriteAllBytes(gif, Convert.FromBase64String(
                "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"));
            var store = new WallpaperLibraryStore(paths);
            var entry = store.Import(gif);
            Assert.Equal("Gif", entry.Kind);
            Assert.Equal(6, ProceduralCatalog.All.Count);
            Assert.True(File.Exists(store.ResolveAbsolute(entry.RelativePath)));
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { /* best-effort */ }
        }
    }
}
