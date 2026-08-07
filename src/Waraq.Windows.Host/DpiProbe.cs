// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 8: high-DPI helpers (PerMonitorV2).

using System.Runtime.InteropServices;

namespace Waraq.Windows.Host;

public static class DpiProbe
{
    [DllImport("user32.dll", EntryPoint = "GetDpiForWindow")]
    private static extern uint GetDpiForWindowNative(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    public static int GetDpiForWindow(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            hwnd = GetDesktopWindow();
        }

        try
        {
            var dpi = GetDpiForWindowNative(hwnd);
            return dpi == 0 ? 96 : (int)dpi;
        }
        catch
        {
            return 96;
        }
    }

    public static double ScaleFactor(IntPtr hwnd) => GetDpiForWindow(hwnd) / 96.0;
}
