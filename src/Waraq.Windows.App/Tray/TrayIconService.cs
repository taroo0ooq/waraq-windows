// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 2: lightweight tray via Shell_NotifyIcon (no WinForms/WPF mix).

using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace Waraq.Windows.App.Tray;

public sealed class TrayIconService : IDisposable
{
    private readonly Window _settingsWindow;
    private readonly IntPtr _hwnd;
    private readonly uint _callbackMessage;
    private bool _added;
    private bool _disposed;
    private SubclassProc? _subclassProc;
    private IntPtr _subclassCookie;

    private const uint NIM_ADD = 0x00000000;
    private const uint NIM_DELETE = 0x00000002;
    private const uint NIF_MESSAGE = 0x00000001;
    private const uint NIF_ICON = 0x00000002;
    private const uint NIF_TIP = 0x00000004;
    private const uint WM_LBUTTONDBLCLK = 0x0203;
    private const uint WM_RBUTTONUP = 0x0205;
    private const uint WM_USER = 0x0400;
    private const int IDI_APPLICATION = 32512;
    private const uint MF_STRING = 0x00000000;
    private const uint MF_SEPARATOR = 0x00000800;
    private const uint TPM_RETURNCMD = 0x0100;

    private const int CMD_SETTINGS = 1;
    private const int CMD_PAUSE = 2;
    private const int CMD_QUIT = 3;

    public TrayIconService(Window settingsWindow)
    {
        _settingsWindow = settingsWindow;
        _hwnd = WindowNative.GetWindowHandle(settingsWindow);
        _callbackMessage = WM_USER + 77;

        var data = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = _callbackMessage,
            hIcon = LoadIcon(IntPtr.Zero, (IntPtr)IDI_APPLICATION),
            szTip = "Waraq for Windows",
        };

        if (!Shell_NotifyIcon(NIM_ADD, ref data))
        {
            throw new InvalidOperationException("Shell_NotifyIcon failed.");
        }

        _added = true;
        _subclassProc = WndProc;
        if (!SetWindowSubclass(_hwnd, _subclassProc, 1, IntPtr.Zero))
        {
            // still keep icon; clicks may not route without subclass
        }
        else
        {
            _subclassCookie = new IntPtr(1);
        }
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
    {
        if (msg == _callbackMessage)
        {
            var mouseMsg = (uint)lParam.ToInt64() & 0xFFFF;
            if (mouseMsg == WM_LBUTTONDBLCLK)
            {
                ShowSettings();
            }
            else if (mouseMsg == WM_RBUTTONUP)
            {
                ShowContextMenu();
            }
        }

        return DefSubclassProc(hWnd, msg, wParam, lParam);
    }

    private void ShowContextMenu()
    {
        var menu = CreatePopupMenu();
        AppendMenu(menu, MF_STRING, new IntPtr(CMD_SETTINGS), "Open settings");
        AppendMenu(menu, MF_STRING, new IntPtr(CMD_PAUSE), "Pause wallpaper (stub)");
        AppendMenu(menu, MF_SEPARATOR, IntPtr.Zero, string.Empty);
        AppendMenu(menu, MF_STRING, new IntPtr(CMD_QUIT), "Quit");

        GetCursorPos(out var pt);
        SetForegroundWindow(_hwnd);
        var cmd = (int)TrackPopupMenu(menu, TPM_RETURNCMD, pt.X, pt.Y, 0, _hwnd, IntPtr.Zero);
        DestroyMenu(menu);

        switch (cmd)
        {
            case CMD_SETTINGS:
                ShowSettings();
                break;
            case CMD_PAUSE:
                // stub — balloon not implemented without more NOTIFYICONDATA flags
                break;
            case CMD_QUIT:
                Dispose();
                Application.Current?.Exit();
                break;
        }
    }

    public void ShowSettings()
    {
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(_hwnd);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        appWindow?.Show();
        _settingsWindow.Activate();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_subclassCookie != IntPtr.Zero && _subclassProc is not null)
        {
            RemoveWindowSubclass(_hwnd, _subclassProc, 1);
        }

        if (_added)
        {
            var data = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _hwnd,
                uID = 1,
            };
            Shell_NotifyIcon(NIM_DELETE, ref data);
            _added = false;
        }
    }

    private delegate IntPtr SubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, IntPtr uIdSubclass, IntPtr dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool RemoveWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, IntPtr uIdSubclass);

    [DllImport("comctl32.dll")]
    private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, IntPtr uIDNewItem, string lpNewItem);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    private static extern uint TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
