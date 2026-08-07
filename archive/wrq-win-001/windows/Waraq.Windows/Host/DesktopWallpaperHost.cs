// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Waraq.Windows.Host;

/// <summary>
/// Locates the desktop WorkerW window and parents wallpaper HWNDs behind icons.
/// See docs/adr/0001-windows-tech-stack-and-wallpaper-host.md.
/// </summary>
public sealed class DesktopWallpaperHost
{
    public string StrategyName => "WorkerW (Progman)";

    /// <summary>Non-destructive probe of Progman / WorkerW.</summary>
    public WallpaperHostProbeResult Probe()
    {
        var progman = NativeMethods.FindWindow("Progman", null);
        if (progman == IntPtr.Zero)
        {
            return new WallpaperHostProbeResult(
                FoundProgman: false,
                FoundWorkerW: false,
                ProgmanHandle: IntPtr.Zero,
                WorkerWHandle: IntPtr.Zero,
                Message: "Progman not found (Explorer shell missing?)");
        }

        EnsureWorkerWSpawned(progman);
        var workerW = FindWallpaperWorkerW();
        return new WallpaperHostProbeResult(
            FoundProgman: true,
            FoundWorkerW: workerW != IntPtr.Zero,
            ProgmanHandle: progman,
            WorkerWHandle: workerW,
            Message: workerW != IntPtr.Zero
                ? "Progman + WorkerW located"
                : "Progman found; WorkerW not located on this shell layout");
    }

    /// <summary>
    /// Finds the WorkerW used as wallpaper parent (sibling after the WorkerW that hosts SHELLDLL_DefView).
    /// </summary>
    public IntPtr FindWallpaperWorkerW()
    {
        var progman = NativeMethods.FindWindow("Progman", null);
        if (progman != IntPtr.Zero)
        {
            EnsureWorkerWSpawned(progman);
        }

        IntPtr workerw = IntPtr.Zero;

        NativeMethods.EnumWindows((topHandle, _) =>
        {
            var shellView = NativeMethods.FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellView != IntPtr.Zero)
            {
                // Standard approach: WorkerW immediately after the desktop DefView owner.
                workerw = NativeMethods.FindWindowEx(IntPtr.Zero, topHandle, "WorkerW", null);
                return false;
            }

            return true;
        }, IntPtr.Zero);

        if (workerw != IntPtr.Zero)
        {
            return workerw;
        }

        // Fallback: first top-level WorkerW that does not host DefView.
        NativeMethods.EnumWindows((topHandle, _) =>
        {
            if (!string.Equals(GetClass(topHandle), "WorkerW", StringComparison.Ordinal))
            {
                return true;
            }

            var shellView = NativeMethods.FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellView == IntPtr.Zero)
            {
                workerw = topHandle;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return workerw;
    }

    /// <summary>
    /// Parents a WPF window under WorkerW and positions it on the virtual desktop bounds (or a monitor rect).
    /// </summary>
    public void AttachWindow(Window window, Rect boundsDip, double dpiScaleX, double dpiScaleY)
    {
        ArgumentNullException.ThrowIfNull(window);

        var workerW = FindWallpaperWorkerW();
        if (workerW == IntPtr.Zero || !NativeMethods.IsWindow(workerW))
        {
            throw new InvalidOperationException(
                "Could not locate desktop WorkerW. Is Explorer running? Try locking/unlocking the workstation or restarting Explorer.");
        }

        var helper = new WindowInteropHelper(window);
        helper.EnsureHandle();
        var hwnd = helper.Handle;

        // Click-through so icons remain usable; toolwindow + noactivate keep it out of alt-tab.
        var ex = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
        ex |= NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_NOINHERITLAYOUT;
        // Intentionally NOT WS_EX_TRANSPARENT on the whole window so MediaElement receives composition;
        // mouse hits pass through because the window is behind SHELLDLL_DefView icons via WorkerW.
        _ = NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, ex);

        _ = NativeMethods.SetParent(hwnd, workerW);

        var x = (int)Math.Round(boundsDip.X * dpiScaleX);
        var y = (int)Math.Round(boundsDip.Y * dpiScaleY);
        var w = Math.Max(1, (int)Math.Round(boundsDip.Width * dpiScaleX));
        var h = Math.Max(1, (int)Math.Round(boundsDip.Height * dpiScaleY));

        _ = NativeMethods.SetWindowPos(
            hwnd,
            NativeMethods.HWND_BOTTOM,
            x,
            y,
            w,
            h,
            NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW | NativeMethods.SWP_FRAMECHANGED);

        _ = NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOWNOACTIVATE);
    }

    /// <summary>Virtual screen bounds in device pixels.</summary>
    public static (int X, int Y, int Width, int Height) GetVirtualScreenPixels()
    {
        var x = NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN);
        var y = NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN);
        var w = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN);
        var h = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN);
        if (w <= 0 || h <= 0)
        {
            w = NativeMethods.GetSystemMetrics(0); // SM_CXSCREEN
            h = NativeMethods.GetSystemMetrics(1); // SM_CYSCREEN
            x = 0;
            y = 0;
        }

        return (x, y, w, h);
    }

    private static void EnsureWorkerWSpawned(IntPtr progman)
    {
        // Magic message used by wallpaper hosts; wParam/lParam variants differ by Windows build.
        _ = NativeMethods.SendMessageTimeout(
            progman,
            NativeMethods.WM_SPAWN_WORKER,
            new UIntPtr(0xD),
            new IntPtr(0x1),
            NativeMethods.SMTO_NORMAL,
            1000,
            out _);

        _ = NativeMethods.SendMessageTimeout(
            progman,
            NativeMethods.WM_SPAWN_WORKER,
            UIntPtr.Zero,
            IntPtr.Zero,
            NativeMethods.SMTO_NORMAL,
            1000,
            out _);
    }

    private static string GetClass(IntPtr hWnd)
    {
        var sb = new StringBuilder(256);
        _ = NativeMethods.GetClassName(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }
}

/// <summary>Result of a non-destructive wallpaper host probe.</summary>
public readonly record struct WallpaperHostProbeResult(
    bool FoundProgman,
    bool FoundWorkerW,
    IntPtr ProgmanHandle,
    IntPtr WorkerWHandle,
    string Message);
