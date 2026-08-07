// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Waraq.Windows.App.HostRuntime;

namespace Waraq.Windows.App;

public partial class App : Application
{
    private Window? _window;

    public static WallpaperController Wallpaper { get; } = new();

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }

    public static string HostRuntimeStatus()
    {
        if (!Wallpaper.IsRunning)
        {
            return "Wallpaper: stopped · host " + Wallpaper.StrategyName;
        }

        return $"Wallpaper: running ({Wallpaper.ActiveKind}) · {Wallpaper.ActivePath}";
    }

    public static void ShutdownWallpaper()
    {
        try
        {
            Wallpaper.Dispose();
        }
        catch
        {
            // best-effort
        }
    }
}
