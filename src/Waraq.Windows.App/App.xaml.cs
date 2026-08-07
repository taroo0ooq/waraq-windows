// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Waraq.Windows.App.HostRuntime;
using Waraq.Windows.Core;

namespace Waraq.Windows.App;

public partial class App : Application
{
    private Window? _window;
    private OnboardingWindow? _onboarding;

    public static WallpaperController Wallpaper { get; } = new();

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        try
        {
            var dq = _window.DispatcherQueue;
            AppServices.GovernorRuntime = new GovernorRuntime(dq);
        }
        catch
        {
            // governor optional if DQ missing
        }

        if (!AppServices.OnboardingState.HasCompleted)
        {
            _onboarding = new OnboardingWindow();
            _onboarding.Activate();
        }

        _window.Activate();
    }

    public static void ShowOnboardingAgain()
    {
        AppServices.OnboardingState.Reset();
        var w = new OnboardingWindow();
        w.Activate();
    }

    public static string HostRuntimeStatus()
    {
        if (!Wallpaper.IsRunning)
        {
            return "Wallpaper: stopped · host " + Wallpaper.StrategyName;
        }

        var pause = Wallpaper.IsPlaybackPaused ? "paused" : "playing";
        return $"Wallpaper: {pause} ({Wallpaper.ActiveKind}) · {Wallpaper.ActivePath ?? Wallpaper.ActiveProceduralId}";
    }

    public static void ShutdownWallpaper()
    {
        try
        {
            AppServices.GovernorRuntime?.Dispose();
            AppServices.GovernorRuntime = null;
            Wallpaper.Dispose();
        }
        catch
        {
            // best-effort
        }
    }
}
