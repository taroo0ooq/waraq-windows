// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 1: shell placeholders. Full NavigationView Settings is Phase 2.

using Waraq.Windows.Core;

namespace Waraq.Windows.Shell;

/// <summary>Sidebar nav labels matching DESIGN.md / settings-shell (Basic mode).</summary>
public static class SettingsNavCatalog
{
    public static IReadOnlyList<string> BasicPanes { get; } =
    [
        "General",
        "Displays",
        "Library",
        "Performance",
        "Wallpapers",
        "Gallery",
        "About",
    ];

    public static string AdvancedOnlyPane => "Diagnostics";
}

public sealed class ScaffoldViewModel
{
    public string Title => AppInfo.PlaceholderTitle;
    public string Status => AppInfo.StatusLine;
    public IReadOnlyList<string> PlannedPanes => SettingsNavCatalog.BasicPanes;
}
