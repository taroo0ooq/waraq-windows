// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 3–7 + HF1-D1: WorkerW surface fail-closed (no blank freeze).

using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using WinRT.Interop;
using Waraq.Windows.Core;
using Waraq.Windows.Engines;
using Waraq.Windows.Engines.Procedural;
using Waraq.Windows.Host;

namespace Waraq.Windows.App.HostRuntime;

public sealed class WallpaperController : IDisposable
{
    private readonly DesktopWallpaperHost _host = new();
    private WallpaperSurfaceWindow? _surface;
    private string? _activePath;
    private string? _activeProceduralId;
    private MediaKind _activeKind = MediaKind.Unknown;
    private bool _disposed;

    public string StrategyName => _host.StrategyName;
    public bool IsRunning => _surface is not null;
    public bool IsPlaybackPaused { get; private set; }
    public bool UserPaused { get; private set; }
    public string? ActivePath => _activePath;
    public string? ActiveProceduralId => _activeProceduralId;
    public MediaKind ActiveKind => _activeKind;
    public string? LastAttachError { get; private set; }

    public WallpaperHostProbeResult Probe() => _host.Probe();

    public void SetUserPaused(bool paused)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        UserPaused = paused;
        ApplyPauseState(paused, force: true);
    }

    public void SetGovernorPaused(bool paused)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (UserPaused)
        {
            ApplyPauseState(true, force: true);
            return;
        }

        ApplyPauseState(paused, force: false);
    }

    private void ApplyPauseState(bool paused, bool force)
    {
        if (_surface is null)
        {
            IsPlaybackPaused = paused;
            return;
        }

        if (!force && IsPlaybackPaused == paused)
        {
            return;
        }

        if (paused)
        {
            _surface.PausePlayback();
        }
        else
        {
            _surface.ResumePlayback();
        }

        IsPlaybackPaused = paused;
    }

    public async Task ApplyAsync(string path, WallpaperFitMode fit = WallpaperFitMode.Fill)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var full = LocalMediaPathGate.NormalizeExistingLocalFile(path);
        var kind = MediaPathClassifier.Classify(full);
        if (!EngineCatalog.IsPhase3Playable(kind) && kind != MediaKind.Image)
        {
            throw new NotSupportedException(
                "Supports local video, GIF, and image files.");
        }

        if (kind is MediaKind.Video or MediaKind.Gif)
        {
            LocalMediaPathGate.EnsureWithinSizeLimit(full, kind);
        }

        StopInternal();

        var window = await CreateAttachedSurfaceAsync().ConfigureAwait(true);
        try
        {
            if (kind == MediaKind.Image)
            {
                await window.InstallMediaAsync(full, MediaKind.Gif, fit).ConfigureAwait(true);
            }
            else
            {
                await window.InstallMediaAsync(full, kind, fit).ConfigureAwait(true);
            }

            _activePath = full;
            _activeProceduralId = null;
            _activeKind = kind == MediaKind.Image ? MediaKind.Image : kind;
            _surface = window;
            LastAttachError = null;
            if (UserPaused)
            {
                ApplyPauseState(true, force: true);
            }
        }
        catch
        {
            DestroyOrphanSurface(window);
            throw;
        }
    }

    public void ApplyProcedural(string engineId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var engine = ProceduralCatalog.Create(engineId);
        engine.Reset();

        StopInternal();

        var window = CreateAttachedSurfaceAsync().GetAwaiter().GetResult();
        try
        {
            window.InstallProcedural(engine);
            _activePath = null;
            _activeProceduralId = engine.Id;
            _activeKind = MediaKind.Procedural;
            _surface = window;
            LastAttachError = null;
            if (UserPaused)
            {
                ApplyPauseState(true, force: true);
            }
        }
        catch
        {
            DestroyOrphanSurface(window);
            throw;
        }
    }

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

    /// <summary>
    /// HF1-D1: create 1×1 hidden surface, attach under WorkerW, only then expand.
    /// Never leaves a top-level black fullscreen if attach fails.
    /// </summary>
    private async Task<WallpaperSurfaceWindow> CreateAttachedSurfaceAsync()
    {
        LastAttachError = null;
        var window = new WallpaperSurfaceWindow();

        // Create HWND without claiming the virtual desktop.
        window.AppWindow.IsShownInSwitchers = false;
        window.AppWindow.Move(new global::Windows.Graphics.PointInt32(-32000, -32000));
        window.AppWindow.Resize(new global::Windows.Graphics.SizeInt32(1, 1));
        try
        {
            window.AppWindow.Hide();
        }
        catch
        {
            // Hide may throw on some builds — off-screen 1x1 still avoids full-screen freeze.
        }

        // Activate is required for HWND materialization on WinUI unpackaged.
        window.Activate();
        await Task.Yield();

        try
        {
            window.AppWindow.Hide();
        }
        catch
        {
            // ignore
        }

        window.AppWindow.Move(new global::Windows.Graphics.PointInt32(-32000, -32000));
        window.AppWindow.Resize(new global::Windows.Graphics.SizeInt32(1, 1));

        var hwnd = WindowNative.GetWindowHandle(window);
        var (vx, vy, vw, vh) = DesktopWallpaperHost.GetVirtualScreenPixels();

        WallpaperAttachResult attach;
        try
        {
            attach = _host.TryAttachHwnd(hwnd, vx, vy, Math.Max(1, vw), Math.Max(1, vh));
        }
        catch (Exception ex)
        {
            DestroyOrphanSurface(window);
            LastAttachError = ex.Message;
            throw new InvalidOperationException(
                "Wallpaper host attach failed (fail-closed). " + ex.Message, ex);
        }

        if (!attach.Success)
        {
            DestroyOrphanSurface(window);
            LastAttachError = attach.Message;
            throw new InvalidOperationException(
                "Wallpaper host attach failed (fail-closed). " + attach.Message);
        }

        return window;
    }

    private static void DestroyOrphanSurface(WallpaperSurfaceWindow window)
    {
        try
        {
            window.Teardown();
        }
        catch
        {
            // ignore
        }

        try
        {
            var hwnd = WindowNative.GetWindowHandle(window);
            if (hwnd != IntPtr.Zero)
            {
                // Best-effort hide + unparent so nothing stays on desktop.
                _ = DesktopWallpaperHostNative.HideAndOrphan(hwnd);
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            window.AppWindow.Hide();
        }
        catch
        {
            // ignore
        }

        try
        {
            window.Close();
        }
        catch
        {
            // ignore
        }
    }

    private void StopInternal()
    {
        if (_surface is not null)
        {
            DestroyOrphanSurface(_surface);
            _surface = null;
        }

        _activePath = null;
        _activeProceduralId = null;
        _activeKind = MediaKind.Unknown;
        IsPlaybackPaused = false;
    }
}

/// <summary>Tiny native helpers used only from fail-closed teardown.</summary>
internal static class DesktopWallpaperHostNative
{
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public static bool HideAndOrphan(IntPtr hwnd)
    {
        try
        {
            ShowWindow(hwnd, 0); // SW_HIDE
            SetParent(hwnd, IntPtr.Zero);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

internal sealed class WallpaperSurfaceWindow : Window
{
    private MediaPlayerElement? _player;
    private Image? _image;
    private MediaPlayer? _mediaPlayer;
    private DispatcherQueueTimer? _timer;
    private WriteableBitmap? _wb;
    private byte[]? _frame;
    private IProceduralEngine? _proc;
    private int _rw;
    private int _rh;
    private readonly System.Diagnostics.Stopwatch _clock = new();
    private bool _paused;

    public WallpaperSurfaceWindow()
    {
        ExtendsContentIntoTitleBar = false;
        Title = "Waraq Wallpaper Surface";
        var presenter = Microsoft.UI.Windowing.OverlappedPresenter.Create();
        presenter.IsResizable = false;
        presenter.IsMinimizable = false;
        presenter.IsMaximizable = false;
        presenter.SetBorderAndTitleBar(false, false);
        AppWindow.SetPresenter(presenter);
        // Transparent until media installs — reduces flash if briefly shown.
        Content = new Grid { Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent) };
    }

    public async Task InstallMediaAsync(string path, MediaKind kind, WallpaperFitMode fit)
    {
        Teardown();
        var root = (Grid)Content!;
        root.Background = new SolidColorBrush(Microsoft.UI.Colors.Black);

        if (kind is MediaKind.Gif or MediaKind.Image)
        {
            _image = new Image
            {
                Stretch = MapStretch(fit),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            var bitmap = new BitmapImage { UriSource = new Uri(path, UriKind.Absolute) };
            _image.Source = bitmap;
            root.Children.Add(_image);
            await Task.CompletedTask.ConfigureAwait(true);
            return;
        }

        var file = await StorageFile.GetFileFromPathAsync(path);
        _mediaPlayer = new MediaPlayer
        {
            IsLoopingEnabled = true,
            AutoPlay = true,
            IsMuted = true,
        };
        _mediaPlayer.Source = MediaSource.CreateFromStorageFile(file);
        _player = new MediaPlayerElement
        {
            AreTransportControlsEnabled = false,
            Stretch = MapStretch(fit),
            AutoPlay = true,
        };
        _player.SetMediaPlayer(_mediaPlayer);
        root.Children.Add(_player);
    }

    public void InstallProcedural(IProceduralEngine engine)
    {
        Teardown();
        _proc = engine;
        var root = (Grid)Content!;
        root.Background = new SolidColorBrush(Microsoft.UI.Colors.Black);

        var (vx, vy, vw, vh) = DesktopWallpaperHost.GetVirtualScreenPixels();
        _ = (vx, vy);
        _rw = Math.Clamp(vw / 3, 320, 960);
        _rh = Math.Clamp(vh / 3, 180, 540);
        _frame = new byte[_rw * _rh * 4];
        _wb = new WriteableBitmap(_rw, _rh);
        _image = new Image
        {
            Source = _wb,
            Stretch = Stretch.Fill,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        root.Children.Add(_image);

        _clock.Restart();
        var dq = DispatcherQueue.GetForCurrentThread();
        _timer = dq.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(33);
        _timer.Tick += (_, _) => RenderProceduralTick();
        _timer.Start();
        RenderProceduralTick();
    }

    public void PausePlayback()
    {
        _paused = true;
        try
        {
            _timer?.Stop();
            _mediaPlayer?.Pause();
        }
        catch
        {
            // ignore
        }
    }

    public void ResumePlayback()
    {
        _paused = false;
        try
        {
            _mediaPlayer?.Play();
            _timer?.Start();
        }
        catch
        {
            // ignore
        }
    }

    private void RenderProceduralTick()
    {
        if (_paused || _proc is null || _wb is null || _frame is null)
        {
            return;
        }

        try
        {
            var t = _clock.Elapsed.TotalSeconds;
            _proc.RenderFrame(_frame, _rw, _rh, t);
            using var stream = _wb.PixelBuffer.AsStream();
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(_frame, 0, _frame.Length);
            _wb.Invalidate();
        }
        catch
        {
            // keep timer alive
        }
    }

    public void Teardown()
    {
        try
        {
            if (_timer is not null)
            {
                _timer.Stop();
                _timer = null;
            }

            _proc = null;
            _frame = null;
            _wb = null;
            _paused = false;

            if (_mediaPlayer is not null)
            {
                _mediaPlayer.Pause();
                _mediaPlayer.Source = null;
                _mediaPlayer.Dispose();
                _mediaPlayer = null;
            }

            if (_player is not null)
            {
                _player.SetMediaPlayer(null);
                _player = null;
            }

            _image = null;
            if (Content is Grid g)
            {
                g.Children.Clear();
                g.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static Stretch MapStretch(WallpaperFitMode fit) => fit switch
    {
        WallpaperFitMode.Stretch => Stretch.Fill,
        WallpaperFitMode.Fit => Stretch.Uniform,
        _ => Stretch.UniformToFill,
    };
}
