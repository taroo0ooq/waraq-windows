// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Waraq.Windows.Core;

namespace Waraq.Windows.Shell;

public enum SettingsPaneId
{
    General,
    Displays,
    Library,
    Performance,
    Wallpapers,
    Gallery,
    Diagnostics,
    About,
}

public sealed record SettingsPaneDescriptor(
    SettingsPaneId Id,
    string Title,
    string Glyph,
    string MacScreenshotRef,
    bool AdvancedOnly = false,
    string StubSummary = "Stub pane. Full controls in later phases.");

public static class SettingsNavCatalog
{
    public static IReadOnlyList<SettingsPaneDescriptor> All { get; } =
    [
        new(SettingsPaneId.General, "General", "\uE713", "docs/screenshots/01-settings-general.png",
            StubSummary: "Startup, language, and basic preferences (stub)."),
        new(SettingsPaneId.Displays, "Displays", "\uE7F4", "docs/screenshots/02-displays.png",
            StubSummary: "Per-monitor wallpapers and profiles (stub)."),
        new(SettingsPaneId.Library, "Library", "\uE8B9", "docs/screenshots/03-library.png",
            StubSummary: "Local library grid and import (stub)."),
        new(SettingsPaneId.Performance, "Performance", "\uE9D9", "docs/screenshots/06-performance.png",
            StubSummary: "Battery, fullscreen pause, quality caps (stub)."),
        new(SettingsPaneId.Wallpapers, "Wallpapers", "\uE91B", "docs/screenshots/03-library.png",
            StubSummary: "Defaults, rotation, transitions (stub)."),
        new(SettingsPaneId.Gallery, "Gallery", "\uE774", "docs/screenshots/04-gallery.png",
            StubSummary: "Pixabay / Pexels / NASA + Browse Web (stub)."),
        new(SettingsPaneId.Diagnostics, "Diagnostics", "\uE9D2", "docs/design/settings-diagnostics.md",
            AdvancedOnly: true,
            StubSummary: "Logs, resource overlay, reset (advanced stub)."),
        new(SettingsPaneId.About, "About", "\uE946", "docs/screenshots/07-about.png",
            StubSummary: "Version, license, credits (stub)."),
    ];

    public static IEnumerable<SettingsPaneDescriptor> Visible(bool advanced) =>
        All.Where(p => advanced || !p.AdvancedOnly);
}

public sealed class SettingsShellViewModel
{
    public string WindowTitle => "Settings";
    public string ProductLine => AppInfo.StatusLine;
    public bool IsAdvancedMode { get; set; }

    public IReadOnlyList<SettingsPaneDescriptor> VisiblePanes =>
        SettingsNavCatalog.Visible(IsAdvancedMode).ToList();

    public SettingsPaneDescriptor DefaultPane =>
        SettingsNavCatalog.All.First(p => p.Id == SettingsPaneId.General);
}

public sealed class ScaffoldViewModel
{
    public string Title => AppInfo.PlaceholderTitle;
    public string Status => AppInfo.StatusLine;
    public IReadOnlyList<string> PlannedPanes =>
        SettingsNavCatalog.All.Where(p => !p.AdvancedOnly).Select(p => p.Title).ToList();
}
