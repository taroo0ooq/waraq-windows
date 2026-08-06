// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Waraq.Windows.Engines;

namespace Waraq.Windows.Host;

/// <summary>
/// Owns wallpaper surface window(s) attached to WorkerW and applies local media.
/// MVP: one surface covering the full virtual desktop (all monitors).
/// </summary>
public sealed class WallpaperController : IDisposable
{
    private readonly DesktopWallpaperHost _host = new();
    private WallpaperSurfaceWindow? _surface;
    private string? _activePath;
    private MediaKind _activeKind = MediaKind.Unknown;
    private WallpaperFitMode _fit = WallpaperFitMode.Fill;
    private bool _disposed;

    public string StrategyName => _host.StrategyName;
    public bool IsRunning => _surface is not null;
    public string? ActivePath => _activePath;
    public MediaKind ActiveKind => _activeKind;
    public WallpaperFitMode FitMode => _fit;

    public WallpaperHostProbeResult Probe() => _host.Probe();

    /// <summary>Apply a local video or GIF as the live wallpaper.</summary>
    public void Apply(string path, WallpaperFitMode fit = WallpaperFitMode.Fill)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Secure path gate: local drive file only (no UNC/URL), size soft-caps.
        var full = LocalMediaPath.NormalizeExistingLocalFile(path);
        var kind = MediaPathClassifier.Classify(full);
        if (kind == MediaKind.Unknown)
        {
            throw new NotSupportedException(
                "Unsupported media type. MVP supports video (.mp4, .webm, .mkv, .avi, .mov, .wmv, .m4v) and .gif.");
        }

        LocalMediaPath.EnsureWithinSizeLimit(full, kind);

        StopInternal();

        _fit = fit;
        _activePath = full;
        _activeKind = kind;

        var window = new WallpaperSurfaceWindow();
        window.Show();

        // DPI of the window's composition target (primary scale is fine for MVP virtual-screen mapping).
        var source = PresentationSource.FromVisual(window);
        var dpiX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        var dpiY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;

        var (vx, vy, vw, vh) = DesktopWallpaperHost.GetVirtualScreenPixels();
        // Convert device pixels → DIP for WPF Left/Top/Width/Height before attach, then host maps back.
        var boundsDip = new Rect(vx / dpiX, vy / dpiY, vw / dpiX, vh / dpiY);
        window.Left = boundsDip.X;
        window.Top = boundsDip.Y;
        window.Width = boundsDip.Width;
        window.Height = boundsDip.Height;

        window.InstallMedia(full, kind, fit);
        _host.AttachWindow(window, boundsDip, dpiX, dpiY);

        _surface = window;
    }

    /// <summary>Stop playback and destroy the wallpaper surface.</summary>
    public void Stop()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        StopInternal();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopInternal();
    }

    private void StopInternal()
    {
        if (_surface is not null)
        {
            try
            {
                _surface.Teardown();
                _surface.Close();
            }
            catch
            {
                // best-effort cleanup
            }

            _surface = null;
        }

        _activePath = null;
        _activeKind = MediaKind.Unknown;
    }
}

/// <summary>Borderless wallpaper HWND content host.</summary>
internal sealed class WallpaperSurfaceWindow : Window
{
    private IDisposable? _engine;

    public WallpaperSurfaceWindow()
    {
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        AllowsTransparency = false;
        Background = Brushes.Black;
        Topmost = false;
        Focusable = false;
        ShowActivated = false;
        Title = "Waraq Wallpaper Surface";
        Content = new Grid { Background = Brushes.Black };
    }

    public void InstallMedia(string path, MediaKind kind, WallpaperFitMode fit)
    {
        Teardown();

        UIElement view;
        if (kind == MediaKind.Gif)
        {
            var gif = new GifWallpaperView();
            gif.Apply(path, fit);
            _engine = gif;
            view = gif;
        }
        else
        {
            var video = new VideoWallpaperView();
            video.Apply(path, fit);
            _engine = video;
            view = video;
        }

        Content = view;
    }

    public void Teardown()
    {
        try
        {
            _engine?.Dispose();
        }
        catch
        {
            // ignore
        }

        _engine = null;
        Content = new Grid { Background = Brushes.Black };
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        // Ensure we never activate / steal focus.
        var ex = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
        ex |= NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW;
        _ = NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, ex);
    }

    protected override void OnClosed(EventArgs e)
    {
        Teardown();
        base.OnClosed(e);
    }
}
