// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002 Phase 7-QA — governor / onboarding / diagnostics policy matrix (Stagecraft QA).

using Waraq.Windows.Core;
using Waraq.Windows.Core.Gallery;
using Waraq.Windows.Engines.Procedural;
using Waraq.Windows.Host;

namespace Waraq.Windows.Tests;

public class GovernorTests
{
    [Fact]
    public void Evaluate_UserPause_Wins_OverBatteryAndFullscreen()
    {
        var settings = new GovernorSettings
        {
            Enabled = true,
            PauseOnBattery = true,
            BatteryThresholdPercent = 50,
            PauseOnFullscreen = true,
            PauseOnHighMemory = true,
            HighMemoryWorkingSetMb = 1,
        };
        var d = PerformanceGovernor.Evaluate(
            settings,
            userPaused: true,
            hasBattery: true,
            batteryPercent: 5,
            isOnBattery: true,
            fullscreenOtherApp: true,
            workingSetBytes: 500L * 1024 * 1024);
        Assert.Equal(GovernorPauseReason.User, d.Reason);
        Assert.True(d.ShouldPause);
    }

    [Fact]
    public void Evaluate_BatteryThreshold()
    {
        var settings = new GovernorSettings
        {
            Enabled = true,
            PauseOnBattery = true,
            BatteryThresholdPercent = 25,
            PauseOnFullscreen = false,
            PauseOnHighMemory = false,
        };
        var d = PerformanceGovernor.Evaluate(settings, false, true, 15, true, false, 1);
        Assert.Equal(GovernorPauseReason.Battery, d.Reason);
        Assert.True(d.ShouldPause);
    }

    [Fact]
    public void Evaluate_Battery_AboveThreshold_Plays()
    {
        var settings = new GovernorSettings
        {
            Enabled = true,
            PauseOnBattery = true,
            BatteryThresholdPercent = 20,
            PauseOnFullscreen = false,
            PauseOnHighMemory = false,
        };
        var d = PerformanceGovernor.Evaluate(settings, false, true, 80, true, false, 1);
        Assert.False(d.ShouldPause);
        Assert.Equal(GovernorPauseReason.None, d.Reason);
    }

    [Fact]
    public void Evaluate_Battery_OnAc_DoesNotPauseForLowPercent()
    {
        var settings = new GovernorSettings
        {
            Enabled = true,
            PauseOnBattery = true,
            BatteryThresholdPercent = 90,
            PauseOnFullscreen = false,
            PauseOnHighMemory = false,
        };
        var d = PerformanceGovernor.Evaluate(settings, false, true, 5, isOnBattery: false, false, 1);
        Assert.False(d.ShouldPause);
    }

    [Fact]
    public void Evaluate_Fullscreen()
    {
        var settings = new GovernorSettings
        {
            Enabled = true,
            PauseOnBattery = false,
            PauseOnFullscreen = true,
            PauseOnHighMemory = false,
        };
        var d = PerformanceGovernor.Evaluate(settings, false, false, -1, false, true, 1);
        Assert.Equal(GovernorPauseReason.Fullscreen, d.Reason);
        Assert.True(d.ShouldPause);
    }

    [Fact]
    public void Evaluate_HighMemory()
    {
        var settings = new GovernorSettings
        {
            Enabled = true,
            PauseOnBattery = false,
            PauseOnFullscreen = false,
            PauseOnHighMemory = true,
            HighMemoryWorkingSetMb = 100,
        };
        var d = PerformanceGovernor.Evaluate(settings, false, false, -1, false, false, 200L * 1024 * 1024);
        Assert.Equal(GovernorPauseReason.HighMemory, d.Reason);
    }

    [Fact]
    public void Evaluate_DisabledGovernor_PlaysEvenIfConditionsMet()
    {
        var settings = new GovernorSettings
        {
            Enabled = false,
            PauseOnBattery = true,
            BatteryThresholdPercent = 100,
            PauseOnFullscreen = true,
            PauseOnHighMemory = true,
            HighMemoryWorkingSetMb = 1,
        };
        var d = PerformanceGovernor.Evaluate(settings, false, true, 1, true, true, 999L * 1024 * 1024);
        Assert.False(d.ShouldPause);
        Assert.Equal(GovernorPauseReason.None, d.Reason);
    }

    [Fact]
    public void Evaluate_PlayingWhenClear()
    {
        var settings = new GovernorSettings
        {
            Enabled = true,
            PauseOnBattery = true,
            BatteryThresholdPercent = 10,
            PauseOnFullscreen = true,
            PauseOnHighMemory = true,
            HighMemoryWorkingSetMb = 4096,
        };
        var d = PerformanceGovernor.Evaluate(settings, false, true, 80, true, false, 50L * 1024 * 1024);
        Assert.False(d.ShouldPause);
        Assert.Equal(GovernorPauseReason.None, d.Reason);
    }

    [Fact]
    public void Evaluate_Priority_FullscreenBeforeBattery()
    {
        // Policy order in PerformanceGovernor: user → enabled → fullscreen → battery → memory
        var settings = new GovernorSettings
        {
            Enabled = true,
            PauseOnBattery = true,
            BatteryThresholdPercent = 50,
            PauseOnFullscreen = true,
            PauseOnHighMemory = false,
        };
        var d = PerformanceGovernor.Evaluate(settings, false, true, 10, true, fullscreenOtherApp: true, 1);
        Assert.Equal(GovernorPauseReason.Fullscreen, d.Reason);
    }

    [Fact]
    public void GovernorSettings_RoundTrip()
    {
        var path = Path.Combine(Path.GetTempPath(), "gov-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var store = new GovernorSettingsStore(path);
            store.Save(new GovernorSettings { BatteryThresholdPercent = 33, Enabled = false });
            var store2 = new GovernorSettingsStore(path);
            Assert.Equal(33, store2.Settings.BatteryThresholdPercent);
            Assert.False(store2.Settings.Enabled);
        }
        finally
        {
            try { File.Delete(path); } catch { /* ok */ }
        }
    }

    [Fact]
    public void Onboarding_FiveSteps_AndCompletionFlag()
    {
        Assert.Equal(5, OnboardingStateStore.Steps.Count);
        Assert.Contains(OnboardingStateStore.Steps, s => s.Contains("Privacy", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(OnboardingStateStore.Steps, s => s.Contains("Performance", StringComparison.OrdinalIgnoreCase));
        var path = Path.Combine(Path.GetTempPath(), "ob-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var store = new OnboardingStateStore(path);
            Assert.False(store.HasCompleted);
            store.MarkCompleted();
            Assert.True(store.HasCompleted);
            store.Reset();
            Assert.False(store.HasCompleted);
        }
        finally
        {
            try { File.Delete(path); } catch { /* ok */ }
        }
    }

    [Fact]
        public void SystemProbes_LocalSamples_DoNotThrow()
        {
            // Diagnostics samples — local only; no telemetry
            var bat = SystemPowerProbe.Sample();
            Assert.True(bat.Percent is >= -1 and <= 100 || !bat.HasBattery);
            var mon = new ResourceMonitor();
            var s1 = mon.Sample();
            var s2 = mon.Sample();
            Assert.True(s2.WorkingSetBytes > 0);
            _ = s1.ProcessCpuPercent;
            _ = FullscreenProbe.IsOtherAppFullscreen();
        }

    [Fact]
    public void HostProbe_NoRegression()
    {
        var probe = new DesktopWallpaperHost().Probe();
        Assert.False(string.IsNullOrWhiteSpace(probe.Message));
    }

    [Fact]
    public void ProceduralAndGallery_NoRegression_WithGovernorPresent()
    {
        Assert.Equal(6, ProceduralCatalog.All.Count);
        Assert.Equal(512L * 1024 * 1024, GalleryUrlPolicy.MaxDownloadBytes);
        Assert.Throws<InvalidOperationException>(() =>
            GalleryUrlPolicy.EnsureSafeHttpsUrl("http://example.com/x.mp4", "qa"));
    }
}
