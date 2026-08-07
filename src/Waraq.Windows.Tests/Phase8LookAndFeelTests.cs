// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002 Phase 8 — L&F / DPI / a11y tests.

using Waraq.Windows.Core;
using Waraq.Windows.Host;
using Waraq.Windows.Shell;

namespace Waraq.Windows.Tests;

public class Phase8LookAndFeelTests
{
    [Fact]
    public void AppManifest_DeclaresPerMonitorV2()
    {
        // Prefer shipped app project manifest next to solution
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
        // Diagnostics advanced-only
        Assert.Contains(SettingsNavCatalog.All, p => p.Id == SettingsPaneId.Diagnostics && p.AdvancedOnly);
        Assert.DoesNotContain(SettingsNavCatalog.Visible(advanced: false), p => p.Id == SettingsPaneId.Diagnostics);
        Assert.Contains(SettingsNavCatalog.Visible(advanced: true), p => p.Id == SettingsPaneId.Diagnostics);
    }

    [Fact]
    public void DpiProbe_Desktop_ReturnsPositiveDpi()
    {
        var dpi = DpiProbe.GetDpiForWindow(IntPtr.Zero);
        Assert.True(dpi >= 96);
        Assert.True(DpiProbe.ScaleFactor(IntPtr.Zero) >= 1.0);
    }

    [Fact]
    public void DesignTokenKeys_DocumentedInPhaseNotes()
    {
        // Sanity: product still phase8-line
        Assert.Contains("phase8", AppInfo.Version, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("L&F", AppInfo.Phase, StringComparison.OrdinalIgnoreCase);
    }
}
