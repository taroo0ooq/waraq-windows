// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 3: WinUI surface attached under WorkerW; video + GIF.

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
using Waraq.Windows.Host;

namespace Waraq.Windows.App.HostRuntime;

/// <summary>
/// Applies local video/GIF as live wallpaper via WorkerW.
/// One surface spans the virtual desktop (multi-monitor MVP).
/// </summary>
public sealed class WallpaperController : IDisposable
{
    private readonly DesktopWallpaperHost _host = new();
    private WallpaperSurfaceWindow? _surface;
    private string? _activePath;
    private MediaKind _activeKind = MediaKind.Unknown;
    private bool _disposed;

    public string StrategyName => _host.StrategyName;
    public bool IsRunning => _surface is not null;
    public string? ActivePath => _activePath;
    public MediaKind ActiveKind => _activeKind;

    public WallpaperHostProbeResult Probe() => _host.Probe();

    public async Task ApplyAsync(string path, WallpaperFitMode fit = WallpaperFitMode.Fill)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Secure path gate: local drive file only (no UNC/URL); size soft-caps.
        var full = LocalMediaPathGate.NormalizeExistingLocalFile(path);
        var kind = MediaPathClassifier.Classify(full);
        if (!EngineCatalog.IsPhase3Playable(kind))
        {
            throw new NotSupportedException(
                "Phase 3 supports local video (.mp4, .webm, .mkv, .avi, .mov, .wmv, .m4v) and .gif only.");
        }

        LocalMediaPathGate.EnsureWithinSizeLimit(full, kind);

        StopInternal();

        _activePath = full;
        _activeKind = kind;

        var window = new WallpaperSurfaceWindow();
        window.Activate();

        var hwnd = WindowNative.GetWindowHandle(window);
        var (vx, vy, vw, vh) = DesktopWallpaperHost.GetVirtualScreenPixels();

        // Position in screen space before parenting (device pixels ≈ WinUI AppWindow).
        window.AppWindow.Move(new global::Windows.Graphics.PointInt32(vx, vy));
        window.AppWindow.Resize(new global::Windows.Graphics.SizeInt32(Math.Max(1, vw), Math.Max(1, vh)));

        await window.InstallMediaAsync(full, kind, fit).ConfigureAwait(true);
        _host.AttachHwnd(hwnd, vx, vy, vw, vh);

        _surface = window;
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
                // best-effort
            }

            _surface = null;
        }

        _activePath = null;
        _activeKind = MediaKind.Unknown;
    }
}

internal sealed class WallpaperSurfaceWindow : Window
{
    private MediaPlayerElement? _player;
    private Image? _gifImage;
    private MediaPlayer? _mediaPlayer;

    public WallpaperSurfaceWindow()
    {
        ExtendsContentIntoTitleBar = false;
        Title = "Waraq Wallpaper Surface";
        // Borderless-ish: hide system chrome via presenter
        var presenter = Microsoft.UI.Windowing.OverlappedPresenter.Create();
        presenter.IsResizable = false;
        presenter.IsMinimizable = false;
        presenter.IsMaximizable = false;
        presenter.SetBorderAndTitleBar(false, false);
        AppWindow.SetPresenter(presenter);

        Content = new Grid { Background = new SolidColorBrush(Microsoft.UI.Colors.Black) };
    }

    public async Task InstallMediaAsync(string path, MediaKind kind, WallpaperFitMode fit)
    {
        Teardown();
        var root = (Grid)Content!;

        if (kind == MediaKind.Gif)
        {
            _gifImage = new Image
            {
                Stretch = MapStretch(fit),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            var bitmap = new BitmapImage();
            // path already validated as local drive file
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            _gifImage.Source = bitmap;
            root.Children.Add(_gifImage);
            await Task.CompletedTask.ConfigureAwait(true);
            return;
        }

        // Video via MediaPlayerElement + MediaPlayer (loop)
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

    public void Teardown()
    {
        try
        {
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

            _gifImage = null;
            if (Content is Grid g)
            {
                g.Children.Clear();
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
