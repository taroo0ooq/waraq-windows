// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Core;

public static class AppInfo
{
    public const string ProductName = "Waraq for Windows";
    public const string Version = "0.6.0-phase6";
    public const string License = "GPL-3.0-or-later";
    public const string WorkId = "WRQ-WIN-002";
    public const string Phase = "6 — gallery + privacy";

    public static string PlaceholderTitle =>
        $"{ProductName} — Phase 6 gallery";

    public static string StatusLine =>
        $"{ProductName} {Version} · {WorkId} · gallery search on demand · zero telemetry";
}
