// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Core;

public static class AppInfo
{
    public const string ProductName = "Waraq for Windows";
    public const string Version = "0.9.1-hf1-d1";
    public const string License = "GPL-3.0-or-later";
    public const string WorkId = "WRQ-WIN-002-HF1";
    public const string Phase = "HF1-D1 — host fail-closed";

    public static string PlaceholderTitle =>
        $"{ProductName} — HF1 host";

    public static string StatusLine =>
        $"{ProductName} {Version} · {WorkId} · fail-closed WorkerW attach";
}
