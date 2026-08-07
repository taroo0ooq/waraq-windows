// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Waraq.Windows.Core;

namespace Waraq.Windows.App.Views;

public sealed partial class PerformancePaneView : UserControl
{
    private bool _loading;

    public PerformancePaneView()
    {
        InitializeComponent();
        Load();
        RefreshStatus();
    }

    private void Load()
    {
        _loading = true;
        var s = AppServices.GovernorSettings.Settings;
        EnabledToggle.IsOn = s.Enabled;
        BatteryToggle.IsOn = s.PauseOnBattery;
        BatteryThreshold.Value = s.BatteryThresholdPercent;
        FullscreenToggle.IsOn = s.PauseOnFullscreen;
        MemoryToggle.IsOn = s.PauseOnHighMemory;
        MemoryLimit.Value = s.HighMemoryWorkingSetMb;
        _loading = false;
    }

    private void OnChanged(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
    }

    private void OnNumberChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (_loading) return;
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var s = new GovernorSettings
        {
            Enabled = EnabledToggle.IsOn,
            PauseOnBattery = BatteryToggle.IsOn,
            BatteryThresholdPercent = (int)Math.Clamp(BatteryThreshold.Value, 5, 100),
            PauseOnFullscreen = FullscreenToggle.IsOn,
            PauseOnHighMemory = MemoryToggle.IsOn,
            HighMemoryWorkingSetMb = (int)Math.Clamp(MemoryLimit.Value, 256, 8192),
        };
        AppServices.GovernorSettings.Save(s);
        AppServices.GovernorRuntime?.Tick();
        StatusText.Text = "Saved governor settings.";
        RefreshStatus();
    }

    private void OnUserPause(object sender, RoutedEventArgs e)
    {
        App.Wallpaper.SetUserPaused(true);
        AppServices.GovernorRuntime?.Tick();
        RefreshStatus();
    }

    private void OnUserResume(object sender, RoutedEventArgs e)
    {
        App.Wallpaper.SetUserPaused(false);
        AppServices.GovernorRuntime?.Tick();
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        var g = AppServices.GovernorRuntime;
        if (g is null)
        {
            StatusText.Text = "Governor runtime not started.";
            return;
        }

        StatusText.Text =
            $"Decision: {g.LastDecision.Reason} — {g.LastDecision.Detail}\n" +
            $"Battery: has={g.LastBattery.HasBattery} onBatt={g.LastBattery.IsOnBattery} pct={g.LastBattery.Percent}\n" +
            $"Fullscreen other: {g.LastFullscreen}\n" +
            $"WS: {g.LastResources.WorkingSetBytes / (1024 * 1024)} MB · CPU~{g.LastResources.ProcessCpuPercent:0.0}%\n" +
            $"Wallpaper paused={App.Wallpaper.IsPlaybackPaused} userPaused={App.Wallpaper.UserPaused}";
    }
}
