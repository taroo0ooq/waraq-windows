// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 7: performance governor settings (local only, zero telemetry).

using System.Text.Json;

namespace Waraq.Windows.Core;

public sealed class GovernorSettings
{
    public bool PauseOnBattery { get; set; } = true;
    public int BatteryThresholdPercent { get; set; } = 20;
    public bool PauseOnFullscreen { get; set; } = true;
    public bool PauseOnHighMemory { get; set; } = true;
    public int HighMemoryWorkingSetMb { get; set; } = 1024;
    public bool Enabled { get; set; } = true;
}

public sealed class GovernorSettingsStore
{
    private readonly string _path;
    private readonly object _gate = new();

    public GovernorSettingsStore(string? path = null)
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Waraq");
        Directory.CreateDirectory(root);
        _path = path ?? Path.Combine(root, "governor.json");
        Settings = Load();
    }

    public GovernorSettings Settings { get; private set; }

    public void Reload() => Settings = Load();

    public void Save(GovernorSettings settings)
    {
        lock (_gate)
        {
            Settings = settings;
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, json);
            File.Copy(tmp, _path, overwrite: true);
            File.Delete(tmp);
        }
    }

    private GovernorSettings Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return new GovernorSettings();
            }

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<GovernorSettings>(json) ?? new GovernorSettings();
        }
        catch
        {
            return new GovernorSettings();
        }
    }
}

public enum GovernorPauseReason
{
    None = 0,
    User,
    Battery,
    Fullscreen,
    HighMemory,
    DisabledPlayback,
}

public sealed class GovernorDecision
{
    public bool ShouldPause { get; init; }
    public GovernorPauseReason Reason { get; init; }
    public string Detail { get; init; } = "";
}

/// <summary>
/// Pure policy evaluator. Windows probes supply samples; no thermal IOKit analogue claimed.
/// </summary>
public static class PerformanceGovernor
{
    public static GovernorDecision Evaluate(
        GovernorSettings settings,
        bool userPaused,
        bool hasBattery,
        int batteryPercent,
        bool isOnBattery,
        bool fullscreenOtherApp,
        long workingSetBytes)
    {
        if (userPaused)
        {
            return new GovernorDecision
            {
                ShouldPause = true,
                Reason = GovernorPauseReason.User,
                Detail = "Paused by user",
            };
        }

        if (!settings.Enabled)
        {
            return new GovernorDecision { ShouldPause = false, Reason = GovernorPauseReason.None, Detail = "Governor off" };
        }

        if (settings.PauseOnFullscreen && fullscreenOtherApp)
        {
            return new GovernorDecision
            {
                ShouldPause = true,
                Reason = GovernorPauseReason.Fullscreen,
                Detail = "Fullscreen app detected",
            };
        }

        if (settings.PauseOnBattery && hasBattery && isOnBattery &&
            batteryPercent >= 0 && batteryPercent <= settings.BatteryThresholdPercent)
        {
            return new GovernorDecision
            {
                ShouldPause = true,
                Reason = GovernorPauseReason.Battery,
                Detail = $"Battery {batteryPercent}% ≤ {settings.BatteryThresholdPercent}%",
            };
        }

        if (settings.PauseOnHighMemory)
        {
            var mb = workingSetBytes / (1024.0 * 1024.0);
            if (mb >= settings.HighMemoryWorkingSetMb)
            {
                return new GovernorDecision
                {
                    ShouldPause = true,
                    Reason = GovernorPauseReason.HighMemory,
                    Detail = $"Working set {mb:0} MB ≥ {settings.HighMemoryWorkingSetMb} MB",
                };
            }
        }

        return new GovernorDecision
        {
            ShouldPause = false,
            Reason = GovernorPauseReason.None,
            Detail = "Playing",
        };
    }
}

public sealed class OnboardingStateStore
{
    private readonly string _path;

    public OnboardingStateStore(string? path = null)
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Waraq");
        Directory.CreateDirectory(root);
        _path = path ?? Path.Combine(root, "onboarding.json");
    }

    public bool HasCompleted
    {
        get
        {
            try
            {
                if (!File.Exists(_path))
                {
                    return false;
                }

                var json = File.ReadAllText(_path);
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.TryGetProperty("completed", out var c) && c.GetBoolean();
            }
            catch
            {
                return false;
            }
        }
    }

    public void MarkCompleted()
    {
        var json = JsonSerializer.Serialize(new { completed = true, atUtc = DateTime.UtcNow });
        File.WriteAllText(_path, json);
    }

    public void Reset()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }

    public static IReadOnlyList<string> Steps { get; } =
    [
        "Welcome to Waraq for Windows",
        "Privacy — network only when you search Gallery",
        "Library — import local video/GIF",
        "Performance — battery & fullscreen pause",
        "You're ready — open Settings anytime from the tray",
    ];
}
