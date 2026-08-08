// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 3 + HF1-D1: WorkerW attach fail-closed (no blank freeze overlay).

using System.Runtime.InteropServices;
using System.Text;

namespace Waraq.Windows.Host;

internal static class NativeMethods
{
    public const uint WM_SPAWN_WORKER = 0x052C;
    public const uint SMTO_NORMAL = 0x0000;
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TOOLWINDOW = 0x00000080;
    public const int WS_EX_NOACTIVATE = 0x08000000;
    public const int WS_EX_NOINHERITLAYOUT = 0x00100000;
    public const int WS_EX_LAYERED = 0x00080000;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_SHOWWINDOW = 0x0040;
    public const uint SWP_HIDEWINDOW = 0x0080;
    public const uint SWP_FRAMECHANGED = 0x0020;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOSIZE = 0x0001;
    public const int SW_HIDE = 0;
    public const int SW_SHOWNOACTIVATE = 4;
    public const int SM_XVIRTUALSCREEN = 76;
    public const int SM_YVIRTUALSCREEN = 77;
    public const int SM_CXVIRTUALSCREEN = 78;
    public const int SM_CYVIRTUALSCREEN = 79;
    public static readonly IntPtr HWND_BOTTOM = new(1);

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

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindow(IntPtr hWnd);
}

public readonly record struct WallpaperHostProbeResult(
    bool FoundProgman,
    bool FoundWorkerW,
    IntPtr ProgmanHandle,
    IntPtr WorkerWHandle,
    string Message);

public readonly record struct WallpaperAttachResult(
    bool Success,
    IntPtr WorkerW,
    IntPtr ParentAfter,
    string Message)
{
    public static WallpaperAttachResult Fail(string message) =>
        new(false, IntPtr.Zero, IntPtr.Zero, message);

    public static WallpaperAttachResult Ok(IntPtr worker, IntPtr parent, string message) =>
        new(true, worker, parent, message);
}

/// <summary>WorkerW discovery + HWND parenting (ADR 0001/0003). HF1-D1 fail-closed.</summary>
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

        EnsureWorkerWSpawned(progman);
        var worker = FindWallpaperWorkerW();
        return new WallpaperHostProbeResult(
            true,
            worker != IntPtr.Zero,
            progman,
            worker,
            worker != IntPtr.Zero
                ? "Progman + WorkerW located"
                : "Progman found; WorkerW not located on this shell layout");
    }

    public IntPtr FindWallpaperWorkerW()
    {
        var progman = NativeMethods.FindWindow("Progman", null);
        if (progman != IntPtr.Zero)
        {
            EnsureWorkerWSpawned(progman);
        }

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

        if (workerw != IntPtr.Zero)
        {
            return workerw;
        }

        NativeMethods.EnumWindows((top, _) =>
        {
            var sb = new StringBuilder(256);
            _ = NativeMethods.GetClassName(top, sb, sb.Capacity);
            if (!string.Equals(sb.ToString(), "WorkerW", StringComparison.Ordinal))
            {
                return true;
            }

            var shellView = NativeMethods.FindWindowEx(top, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellView == IntPtr.Zero)
            {
                workerw = top;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return workerw;
    }

    /// <summary>
    /// Parent under WorkerW and size. Verifies GetParent == WorkerW.
    /// Does not leave the HWND as a top-level fullscreen window on failure.
    /// </summary>
    public WallpaperAttachResult TryAttachHwnd(IntPtr hwnd, int xPx, int yPx, int widthPx, int heightPx)
    {
        if (hwnd == IntPtr.Zero || !NativeMethods.IsWindow(hwnd))
        {
            return WallpaperAttachResult.Fail("Invalid HWND.");
        }

        var workerW = FindWallpaperWorkerW();
        if (workerW == IntPtr.Zero || !NativeMethods.IsWindow(workerW))
        {
            return WallpaperAttachResult.Fail(
                "Could not locate desktop WorkerW. Is Explorer running?");
        }

        // Keep hidden while parenting — caller should already have a 1x1 off-screen window.
        _ = NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);

        var ex = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
        ex |= NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_NOINHERITLAYOUT;
        _ = NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, ex);

        var prevParent = NativeMethods.SetParent(hwnd, workerW);
        _ = prevParent;
        var parentAfter = NativeMethods.GetParent(hwnd);
        if (parentAfter != workerW)
        {
            // Detach best-effort and leave hidden — never show unparented fullscreen.
            try
            {
                _ = NativeMethods.SetParent(hwnd, IntPtr.Zero);
            }
            catch
            {
                // ignore
            }

            _ = NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);
            return WallpaperAttachResult.Fail(
                $"WorkerW SetParent failed or was ignored (parent=0x{parentAfter.ToInt64():X}, worker=0x{workerW.ToInt64():X}). WinUI may refuse reparent.");
        }

        _ = NativeMethods.SetWindowPos(
            hwnd,
            NativeMethods.HWND_BOTTOM,
            xPx,
            yPx,
            Math.Max(1, widthPx),
            Math.Max(1, heightPx),
            NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW | NativeMethods.SWP_FRAMECHANGED);
        _ = NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOWNOACTIVATE);

        // Re-check parent after show/size
        parentAfter = NativeMethods.GetParent(hwnd);
        if (parentAfter != workerW)
        {
            _ = NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);
            try { _ = NativeMethods.SetParent(hwnd, IntPtr.Zero); } catch { /* ignore */ }
            return WallpaperAttachResult.Fail(
                "Parent lost after size/show — fail-closed (no desktop overlay).");
        }

        return WallpaperAttachResult.Ok(workerW, parentAfter, "Attached under WorkerW");
    }

    /// <summary>Legacy throw-on-fail wrapper.</summary>
    public void AttachHwnd(IntPtr hwnd, int xPx, int yPx, int widthPx, int heightPx)
    {
        var r = TryAttachHwnd(hwnd, xPx, yPx, widthPx, heightPx);
        if (!r.Success)
        {
            throw new InvalidOperationException(r.Message);
        }
    }

    public static (int X, int Y, int Width, int Height) GetVirtualScreenPixels()
    {
        var x = NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN);
        var y = NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN);
        var w = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN);
        var h = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN);
        if (w <= 0 || h <= 0)
        {
            w = NativeMethods.GetSystemMetrics(0);
            h = NativeMethods.GetSystemMetrics(1);
            x = 0;
            y = 0;
        }

        return (x, y, w, h);
    }

    private static void EnsureWorkerWSpawned(IntPtr progman)
    {
        _ = NativeMethods.SendMessageTimeout(
            progman, NativeMethods.WM_SPAWN_WORKER, new UIntPtr(0xD), new IntPtr(0x1),
            NativeMethods.SMTO_NORMAL, 1000, out _);
        _ = NativeMethods.SendMessageTimeout(
            progman, NativeMethods.WM_SPAWN_WORKER, UIntPtr.Zero, IntPtr.Zero,
            NativeMethods.SMTO_NORMAL, 1000, out _);
    }
}
