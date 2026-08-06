// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Text;

namespace Waraq.Windows.Host;

/// <summary>
/// Locates the desktop WorkerW window used to host live wallpaper surfaces.
/// Phase 1a: discovery helpers only — no SetParent / live surface yet (Phase 2).
/// See docs/adr/0001-windows-tech-stack-and-wallpaper-host.md.
/// </summary>
public sealed class DesktopWallpaperHost
{
    /// <summary>Strategy name for diagnostics and ADR alignment.</summary>
    public string StrategyName => "WorkerW (Progman)";

    /// <summary>
    /// Attempts to find Progman and optionally ping it to ensure WorkerW exists.
    /// Does not create or parent any wallpaper HWND in Phase 1a.
    /// </summary>
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

        // Ask Progman to create the WorkerW pair used by wallpaper hosts.
        // Safe no-op-ish on modern Windows if already created; still version-sensitive.
        _ = NativeMethods.SendMessageTimeout(
            progman,
            NativeMethods.WM_SPAWN_WORKER,
            UIntPtr.Zero,
            IntPtr.Zero,
            NativeMethods.SMTO_NORMAL,
            1000,
            out _);

        var workerW = FindWorkerWBehindDesktopIcons();
        return new WallpaperHostProbeResult(
            FoundProgman: true,
            FoundWorkerW: workerW != IntPtr.Zero,
            ProgmanHandle: progman,
            WorkerWHandle: workerW,
            Message: workerW != IntPtr.Zero
                ? "Progman + WorkerW located (host attach deferred to Phase 2)"
                : "Progman found; WorkerW not located on this shell layout (will need Win10/11 path variants)");
    }

    /// <summary>
    /// Walks top-level windows for WorkerW that hosts SHELLDLL_DefView or its sibling layout.
    /// Layout differs across Windows 10/11 builds — this is intentionally conservative.
    /// </summary>
    public IntPtr FindWorkerWBehindDesktopIcons()
    {
        IntPtr found = IntPtr.Zero;

        NativeMethods.EnumWindows((hWnd, _) =>
        {
            var className = GetClass(hWnd);
            if (!string.Equals(className, "WorkerW", StringComparison.Ordinal))
            {
                return true;
            }

            // Common pattern: WorkerW contains SHELLDLL_DefView (icons).
            var defView = NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (defView != IntPtr.Zero)
            {
                // On many builds the wallpaper WorkerW is a *sibling* after the one with DefView.
                // Continue enumeration; remember the DefView owner and prefer next WorkerW if needed.
                // For probe purposes, accepting any WorkerW that participates in the desktop tree is enough.
                found = hWnd;
                return true;
            }

            // Some builds: WorkerW without DefView is the wallpaper surface parent.
            if (found == IntPtr.Zero)
            {
                found = hWnd;
            }

            return true;
        }, IntPtr.Zero);

        return found;
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
