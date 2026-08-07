// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002 Phase 7 — governor + onboarding tests.

using Waraq.Windows.Core;

namespace Waraq.Windows.Tests;

public class GovernorTests
{
    [Fact]
    public void Evaluate_UserPause_Wins()
    {
        var d = PerformanceGovernor.Evaluate(
            new GovernorSettings { Enabled = true },
            userPaused: true,
            hasBattery: true,
            batteryPercent: 100,
            isOnBattery: false,
            fullscreenOtherApp: false,
            workingSetBytes: 1);
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
}
