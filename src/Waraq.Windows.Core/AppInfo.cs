// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Core;

public static class AppInfo
{
    public const string ProductName = "Waraq for Windows";
    public const string Version = "0.8.0-phase8";
    public const string License = "GPL-3.0-or-later";
    public const string WorkId = "WRQ-WIN-002";
    public const string Phase = "8 — L&F + a11y + DPI";

    public static string PlaceholderTitle =>
        $"{ProductName} — Phase 8 L&F";

    public static string StatusLine =>
        $"{ProductName} {Version} · {WorkId} · Fluent L&F · PerMonitorV2 · a11y";
}
