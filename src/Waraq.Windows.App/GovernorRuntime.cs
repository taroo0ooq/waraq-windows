// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 7: poll probes and apply governor pause to wallpaper host.

using Microsoft.UI.Dispatching;
using Waraq.Windows.Core;
using Waraq.Windows.Host;

namespace Waraq.Windows.App;

public sealed class GovernorRuntime : IDisposable
{
    private readonly ResourceMonitor _resources = new();
    private readonly DispatcherQueueTimer _timer;
    private bool _disposed;

    public GovernorRuntime(DispatcherQueue dq)
    {
        LastDecision = new GovernorDecision
        {
            ShouldPause = false,
            Reason = GovernorPauseReason.None,
            Detail = "Idle",
        };
        LastBattery = SystemPowerProbe.Sample();
        LastResources = _resources.Sample();

        _timer = dq.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(5);
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
        Tick();
    }

    public GovernorDecision LastDecision { get; private set; }
    public BatterySample LastBattery { get; private set; }
    public ResourceSample LastResources { get; private set; }
    public bool LastFullscreen { get; private set; }

    public void Tick()
    {
        if (_disposed)
        {
            return;
        }

        AppServices.GovernorSettings.Reload();
        var settings = AppServices.GovernorSettings.Settings;
        LastBattery = SystemPowerProbe.Sample();
        LastResources = _resources.Sample();
        LastFullscreen = FullscreenProbe.IsOtherAppFullscreen();

        LastDecision = PerformanceGovernor.Evaluate(
            settings,
            App.Wallpaper.UserPaused,
            LastBattery.HasBattery,
            LastBattery.Percent,
            LastBattery.IsOnBattery,
            LastFullscreen,
            LastResources.WorkingSetBytes);

        try
        {
            if (App.Wallpaper.IsRunning)
            {
                App.Wallpaper.SetGovernorPaused(LastDecision.ShouldPause);
            }
        }
        catch
        {
            // best-effort
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            _timer.Stop();
        }
        catch
        {
            // ignore
        }
    }
}
