// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Windows;

namespace Waraq.Windows;

/// <summary>
/// Application entry. Ensures wallpaper surfaces are torn down on exit.
/// </summary>
public partial class App : Application
{
    /// <summary>Shared controller for the settings shell lifetime.</summary>
    public static Host.WallpaperController Wallpaper { get; } = new();

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            Wallpaper.Dispose();
        }
        catch
        {
            // best-effort
        }

        base.OnExit(e);
    }
}
