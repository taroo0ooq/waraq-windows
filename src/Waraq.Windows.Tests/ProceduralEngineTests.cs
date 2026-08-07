// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002 Phase 5 — procedural engines tests.

using Waraq.Windows.Engines;
using Waraq.Windows.Engines.Procedural;

namespace Waraq.Windows.Tests;

public class ProceduralEngineTests
{
    [Fact]
    public void Catalog_HasSixEngines_MatchingMacSet()
    {
        Assert.Equal(6, ProceduralCatalog.All.Count);
        Assert.Contains(ProceduralCatalog.All, d => d.Id == "aurora");
        Assert.Contains(ProceduralCatalog.All, d => d.Id == "matrix-rain");
        Assert.Contains(ProceduralCatalog.All, d => d.Id == "synthwave");
        Assert.Contains(ProceduralCatalog.All, d => d.Id == "starfield");
        Assert.Contains(ProceduralCatalog.All, d => d.Id == "neural-network");
        Assert.Contains(ProceduralCatalog.All, d => d.Id == "animated-gradient");
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

    [Fact]
    public void UnknownEngine_Throws()
    {
        Assert.Throws<ArgumentException>(() => ProceduralCatalog.Create("nope"));
    }

    [Fact]
    public void EngineCatalog_ListsProcedural()
    {
        Assert.Equal(6, EngineCatalog.ProceduralEngines.Count);
        Assert.Contains(EngineCatalog.PlannedEngines, e => e.Contains("Aurora", StringComparison.Ordinal));
    }
}
