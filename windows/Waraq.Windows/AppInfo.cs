// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Waraq.Windows;

/// <summary>Build and product metadata for UI and tests.</summary>
public static class AppInfo
{
    public const string ProductName = "Waraq for Windows";
    public const string Version = "0.1.0-alpha";
    public const string License = "GPL-3.0-or-later";
    public const string UpstreamUrl = "https://github.com/bahamut42/waraq";
    public const string RepoUrl = "https://github.com/taroo0ooq/waraq-windows";

    /// <summary>
    /// Single-line status for the Phase 1a shell.
    /// Wallpaper host is scaffolded but not attached to WorkerW yet.
    /// </summary>
    public static string ScaffoldStatus =>
        $"{ProductName} {Version} — scaffold (WorkerW host not attached)";
}
