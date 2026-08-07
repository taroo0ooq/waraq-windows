// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Core;

public static class AppInfo
{
    public const string ProductName = "Waraq for Windows";
    public const string Version = "0.2.0-phase2";
    public const string License = "GPL-3.0-or-later";
    public const string WorkId = "WRQ-WIN-002";
    public const string Phase = "2 — design shell";

    public static string PlaceholderTitle =>
        $"{ProductName} — Phase 2 Settings shell";

    public static string StatusLine =>
        $"{ProductName} {Version} · {WorkId} · tray + NavigationView stubs";
}
