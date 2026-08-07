// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 1: compile-only host surface. Live WorkerW attach is Phase 3.

using System.Runtime.InteropServices;
using System.Text;

namespace Waraq.Windows.Host;

internal static class NativeMethods
{
    public const uint WM_SPAWN_WORKER = 0x052C;

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SendMessageTimeout(
        IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam,
        uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    public const uint SMTO_NORMAL = 0x0000;
}

/// <summary>
/// WorkerW discovery helpers (reference: archive/wrq-win-001 + ADR 0001/0003).
/// Phase 1 does not SetParent wallpaper HWNDs.
/// </summary>
public sealed class DesktopWallpaperHost
{
    public string StrategyName => "WorkerW (Progman)";

    public WallpaperHostProbeResult Probe()
    {
        var progman = NativeMethods.FindWindow("Progman", null);
        if (progman == IntPtr.Zero)
        {
            return new WallpaperHostProbeResult(false, false, IntPtr.Zero, IntPtr.Zero,
                "Progman not found (Explorer shell missing?)");
        }

        _ = NativeMethods.SendMessageTimeout(
            progman, NativeMethods.WM_SPAWN_WORKER, new UIntPtr(0xD), new IntPtr(0x1),
            NativeMethods.SMTO_NORMAL, 1000, out _);

        var worker = FindWallpaperWorkerW();
        return new WallpaperHostProbeResult(
            true,
            worker != IntPtr.Zero,
            progman,
            worker,
            worker != IntPtr.Zero
                ? "Progman + WorkerW located (attach deferred to Phase 3)"
                : "Progman found; WorkerW not located on this shell layout");
    }

    public IntPtr FindWallpaperWorkerW()
    {
        IntPtr workerw = IntPtr.Zero;
        NativeMethods.EnumWindows((top, _) =>
        {
            var shellView = NativeMethods.FindWindowEx(top, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellView != IntPtr.Zero)
            {
                workerw = NativeMethods.FindWindowEx(IntPtr.Zero, top, "WorkerW", null);
                return false;
            }

            return true;
        }, IntPtr.Zero);
        return workerw;
    }
}

public readonly record struct WallpaperHostProbeResult(
    bool FoundProgman,
    bool FoundWorkerW,
    IntPtr ProgmanHandle,
    IntPtr WorkerWHandle,
    string Message);
