// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Core;

public static class AppInfo
{
    public const string ProductName = "Waraq for Windows";
    public const string Version = "0.1.0-phase1";
    public const string License = "GPL-3.0-or-later";
    public const string WorkId = "WRQ-WIN-002";
    public const string Phase = "1 — scaffold";

    public static string PlaceholderTitle =>
        $"{ProductName} — Phase 1 scaffold (WinUI 3)";

    public static string StatusLine =>
        $"{ProductName} {Version} · {WorkId} · empty shell (no Settings parity yet)";
}
