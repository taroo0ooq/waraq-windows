// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Core;

public static class AppInfo
{
    public const string ProductName = "Waraq for Windows";
    public const string Version = "0.4.0-phase4";
    public const string License = "GPL-3.0-or-later";
    public const string WorkId = "WRQ-WIN-002";
    public const string Phase = "4 — library + profiles";

    public static string PlaceholderTitle =>
        $"{ProductName} — Phase 4 library";

    public static string StatusLine =>
        $"{ProductName} {Version} · {WorkId} · library import + display profiles";
}
