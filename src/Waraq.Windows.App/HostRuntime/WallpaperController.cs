// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 3–5: WorkerW surface — video/GIF/procedural.

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
    public string? ActivePath => _activePath;
    public string? ActiveProceduralId => _activeProceduralId;
    public MediaKind ActiveKind => _activeKind;

    public WallpaperHostProbeResult Probe() => _host.Probe();

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

        _activePath = full;
        _activeProceduralId = null;
        _activeKind = kind == MediaKind.Image ? MediaKind.Image : kind;

        var window = await CreateAttachedSurfaceAsync().ConfigureAwait(true);
        if (kind == MediaKind.Image)
        {
            // treat static image like GIF path (BitmapImage)
            await window.InstallMediaAsync(full, MediaKind.Gif, fit).ConfigureAwait(true);
        }
        else
        {
            await window.InstallMediaAsync(full, kind, fit).ConfigureAwait(true);
        }

        _surface = window;
    }

    public void ApplyProcedural(string engineId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var engine = ProceduralCatalog.Create(engineId);
        engine.Reset();

        StopInternal();

        _activePath = null;
        _activeProceduralId = engine.Id;
        _activeKind = MediaKind.Procedural;

        // sync attach
        var window = CreateAttachedSurfaceAsync().GetAwaiter().GetResult();
        window.InstallProcedural(engine);
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

    private async Task<WallpaperSurfaceWindow> CreateAttachedSurfaceAsync()
    {
        var window = new WallpaperSurfaceWindow();
        window.Activate();

        var hwnd = WindowNative.GetWindowHandle(window);
        var (vx, vy, vw, vh) = DesktopWallpaperHost.GetVirtualScreenPixels();
        window.AppWindow.Move(new global::Windows.Graphics.PointInt32(vx, vy));
        window.AppWindow.Resize(new global::Windows.Graphics.SizeInt32(Math.Max(1, vw), Math.Max(1, vh)));
        await Task.Yield();
        _host.AttachHwnd(hwnd, vx, vy, vw, vh);
        return window;
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
        _activeProceduralId = null;
        _activeKind = MediaKind.Unknown;
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
        Content = new Grid { Background = new SolidColorBrush(Microsoft.UI.Colors.Black) };
    }

    public async Task InstallMediaAsync(string path, MediaKind kind, WallpaperFitMode fit)
    {
        Teardown();
        var root = (Grid)Content!;

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

        // Render at reduced resolution for CPU budget; stretch to desktop.
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
        _timer.Interval = TimeSpan.FromMilliseconds(33); // ~30fps
        _timer.Tick += (_, _) => RenderProceduralTick();
        _timer.Start();
        RenderProceduralTick();
    }

    private void RenderProceduralTick()
    {
        if (_proc is null || _wb is null || _frame is null)
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
