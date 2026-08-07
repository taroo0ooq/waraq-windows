// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Stable display keys via EnumDisplayDevices (DeviceID), not volatile GDI index alone.

using System.Runtime.InteropServices;
using System.Text;

namespace Waraq.Windows.Host;

public readonly record struct DisplayInfo(string Key, string FriendlyName, string DeviceName, int GdiIndex);

public static class DisplayEnumerator
{
    private const int CCHDEVICENAME = 32;
    private const int EDD_GET_DEVICE_INTERFACE_NAME = 0x00000001;

    public static IReadOnlyList<DisplayInfo> EnumerateActiveDisplays()
    {
        var list = new List<DisplayInfo>();
        for (uint i = 0; ; i++)
        {
            var dd = new DISPLAY_DEVICE
            {
                cb = Marshal.SizeOf<DISPLAY_DEVICE>(),
            };

            if (!EnumDisplayDevices(null, i, ref dd, 0))
            {
                break;
            }

            // DISPLAY_DEVICE_ACTIVE = 0x1, ATTACHED_TO_DESKTOP = 0x1 historically on some builds
            const uint DISPLAY_DEVICE_ACTIVE = 0x00000001;
            const uint DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001;
            if ((dd.StateFlags & (DISPLAY_DEVICE_ACTIVE | DISPLAY_DEVICE_ATTACHED_TO_DESKTOP)) == 0)
            {
                continue;
            }

            var deviceName = dd.DeviceName ?? $"\\\\.\\DISPLAY{i + 1}";
            var mon = new DISPLAY_DEVICE { cb = Marshal.SizeOf<DISPLAY_DEVICE>() };
            string key;
            string friendly;
            if (EnumDisplayDevices(deviceName, 0, ref mon, EDD_GET_DEVICE_INTERFACE_NAME))
            {
                // Prefer DeviceID (stable PNP) then DeviceKey
                key = FirstNonEmpty(mon.DeviceID, mon.DeviceKey, mon.DeviceString, deviceName)!;
                friendly = FirstNonEmpty(mon.DeviceString, dd.DeviceString, deviceName)!;
            }
            else
            {
                key = FirstNonEmpty(dd.DeviceID, dd.DeviceKey, deviceName)!;
                friendly = FirstNonEmpty(dd.DeviceString, deviceName)!;
            }

            // Normalize key
            key = key.Trim();
            list.Add(new DisplayInfo(key, friendly.Trim(), deviceName, (int)i));
        }

        if (list.Count == 0)
        {
            list.Add(new DisplayInfo("PRIMARY", "Primary display", "\\\\.\\DISPLAY1", 0));
        }

        return list;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
            {
                return v;
            }
        }

        return null;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DISPLAY_DEVICE
    {
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }
}
