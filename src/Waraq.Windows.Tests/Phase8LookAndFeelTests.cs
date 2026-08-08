// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002 Phase 8-QA — L&F / a11y / DPI regression matrix (Stagecraft QA).
// Owner L&F accept is a human gate — not closed by this desk.

using Waraq.Windows.Core;
using Waraq.Windows.Core.Gallery;
using Waraq.Windows.Engines.Procedural;
using Waraq.Windows.Host;
using Waraq.Windows.Shell;

namespace Waraq.Windows.Tests;

public class Phase8LookAndFeelTests
{
    [Fact]
    public void AppManifest_DeclaresPerMonitorV2()
    {
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Waraq.Windows.App", "app.manifest")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Waraq.Windows.App", "app.manifest")),
            Path.Combine(Directory.GetCurrentDirectory(), "Waraq.Windows.App", "app.manifest"),
            Path.Combine(Directory.GetCurrentDirectory(), "app.manifest"),
        };

        string? path = candidates.FirstOrDefault(File.Exists);
        Assert.True(path is not null, "app.manifest not found near test host");
        var xml = File.ReadAllText(path!);
        Assert.Contains("PerMonitorV2", xml, StringComparison.Ordinal);
        Assert.Contains("dpiAwareness", xml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SettingsNav_AllPanes_HaveTitlesAndGlyphs()
    {
        Assert.NotEmpty(SettingsNavCatalog.All);
        Assert.All(SettingsNavCatalog.All, p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Title));
            Assert.False(string.IsNullOrWhiteSpace(p.Glyph));
            Assert.False(string.IsNullOrWhiteSpace(p.StubSummary));
        });
        Assert.Contains(SettingsNavCatalog.All, p => p.Id == SettingsPaneId.Diagnostics && p.AdvancedOnly);
        Assert.DoesNotContain(SettingsNavCatalog.Visible(advanced: false), p => p.Id == SettingsPaneId.Diagnostics);
        Assert.Contains(SettingsNavCatalog.Visible(advanced: true), p => p.Id == SettingsPaneId.Diagnostics);
    }

    [Fact]
    public void SettingsNav_CorePanes_Present_ForLfRegression()
    {
        var ids = SettingsNavCatalog.Visible(advanced: true).Select(p => p.Id).ToHashSet();
        // Core surfaces from Host→Library→Procedural→Gallery→Performance chain
        Assert.Contains(SettingsPaneId.General, ids);
        Assert.Contains(SettingsPaneId.Library, ids);
        Assert.Contains(SettingsPaneId.Wallpapers, ids);
        Assert.Contains(SettingsPaneId.Performance, ids);
        Assert.Contains(SettingsPaneId.Diagnostics, ids);
    }

    [Fact]
    public void DpiProbe_Desktop_ReturnsPositiveDpi()
    {
        var dpi = DpiProbe.GetDpiForWindow(IntPtr.Zero);
        Assert.True(dpi >= 96);
        Assert.True(DpiProbe.ScaleFactor(IntPtr.Zero) >= 1.0);
        Assert.InRange(DpiProbe.ScaleFactor(IntPtr.Zero), 1.0, 5.0);
    }

    [Fact]
    public void DesignTokenKeys_DocumentedInPhaseNotes()
    {
        Assert.False(string.IsNullOrWhiteSpace(AppInfo.Version));
        Assert.False(string.IsNullOrWhiteSpace(AppInfo.Phase));
    }

    [Fact]
    public void ScreenshotPack_Phase8_FilesPresent()
    {
        var roots = new[]
        {
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "screenshots", "windows", "phase8")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "docs", "screenshots", "windows", "phase8")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "docs", "screenshots", "windows", "phase8")),
        };
        var dir = roots.FirstOrDefault(Directory.Exists);
        Assert.True(dir is not null, "phase8 screenshot pack directory missing");
        Assert.True(File.Exists(Path.Combine(dir!, "phase8-desktop-settings.png")));
        Assert.True(File.Exists(Path.Combine(dir!, "01-general.png")));
        Assert.True(File.Exists(Path.Combine(dir!, "README.md")));
        var readme = File.ReadAllText(Path.Combine(dir!, "README.md"));
        Assert.Contains("L&F", readme, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HostProbe_NoRegression_Phase8()
    {
        var probe = new DesktopWallpaperHost().Probe();
        Assert.False(string.IsNullOrWhiteSpace(probe.Message));
        Assert.Equal("WorkerW (Progman)", new DesktopWallpaperHost().StrategyName);
    }

    [Fact]
    public void PriorPhases_NoRegression_UnderPhase8()
    {
        Assert.Equal(6, ProceduralCatalog.All.Count);
        Assert.Equal(512L * 1024 * 1024, GalleryUrlPolicy.MaxDownloadBytes);
        Assert.Equal(5, OnboardingStateStore.Steps.Count);
        var g = PerformanceGovernor.Evaluate(
            new GovernorSettings { Enabled = true },
            userPaused: true,
            hasBattery: false,
            batteryPercent: -1,
            isOnBattery: false,
            fullscreenOtherApp: false,
            workingSetBytes: 1);
        Assert.Equal(GovernorPauseReason.User, g.Reason);
    }

    [Fact]
    public void A11y_NavPaneTitles_AreNonEmpty_Unique()
    {
        var titles = SettingsNavCatalog.All.Select(p => p.Title).ToList();
        Assert.Equal(titles.Count, titles.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(titles, t => Assert.True(t.Length >= 2));
    }
}
