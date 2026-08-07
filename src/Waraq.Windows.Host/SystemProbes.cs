// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 7: local system probes (no telemetry).

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Waraq.Windows.Host;

public readonly record struct BatterySample(bool HasBattery, bool IsOnBattery, int Percent);

public readonly record struct ResourceSample(
    double ProcessCpuPercent,
    long WorkingSetBytes,
    long PrivateBytes,
    int ThreadCount);

public static class SystemPowerProbe
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_POWER_STATUS
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;
    }

    [DllImport("kernel32.dll")]
    private static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS sps);

    public static BatterySample Sample()
    {
        if (!GetSystemPowerStatus(out var sps))
        {
            return new BatterySample(false, false, -1);
        }

        // BatteryFlag 128 = no system battery
        var hasBattery = (sps.BatteryFlag & 128) == 0;
        var onBattery = sps.ACLineStatus == 0;
        int pct = sps.BatteryLifePercent is >= 0 and <= 100 ? sps.BatteryLifePercent : -1;
        if (!hasBattery)
        {
            return new BatterySample(false, false, -1);
        }

        return new BatterySample(true, onBattery, pct);
    }
}

public static class FullscreenProbe
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    /// <summary>
    /// True when another process owns a near-fullscreen foreground window
    /// covering the primary monitor (honest heuristic; not IOKit-perfect).
    /// </summary>
    public static bool IsOtherAppFullscreen()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        GetWindowThreadProcessId(hwnd, out var pid);
        if (pid == (uint)Environment.ProcessId)
        {
            return false;
        }

        if (!GetWindowRect(hwnd, out var r))
        {
            return false;
        }

        var (vx, vy, vw, vh) = DesktopWallpaperHost.GetVirtualScreenPixels();
        // Primary-ish: use virtual bounds; require ≥95% coverage of virtual desktop
        var area = Math.Max(1L, (long)vw * vh);
        var w = Math.Max(0, r.Right - r.Left);
        var h = Math.Max(0, r.Bottom - r.Top);
        var cover = (double)w * h / area;
        if (cover < 0.92)
        {
            return false;
        }

        // Exclude tiny tool windows that maximize oddly
        return w >= vw * 0.9 && h >= vh * 0.9;
    }
}

public sealed class ResourceMonitor
{
    private readonly Process _proc = Process.GetCurrentProcess();
    private TimeSpan _lastCpu;
    private DateTime _lastWall = DateTime.UtcNow;

    public ResourceSample Sample()
    {
        _proc.Refresh();
        var now = DateTime.UtcNow;
        var cpu = _proc.TotalProcessorTime;
        var wall = now - _lastWall;
        double pct = 0;
        if (wall.TotalMilliseconds > 50)
        {
            var deltaCpu = (cpu - _lastCpu).TotalMilliseconds;
            pct = deltaCpu / (wall.TotalMilliseconds * Environment.ProcessorCount) * 100.0;
            pct = Math.Clamp(pct, 0, 100);
        }

        _lastCpu = cpu;
        _lastWall = now;

        return new ResourceSample(
            pct,
            _proc.WorkingSet64,
            _proc.PrivateMemorySize64,
            _proc.Threads.Count);
    }
}
