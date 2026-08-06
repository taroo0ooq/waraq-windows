// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Waraq.Windows.Engines;

/// <summary>Stretch modes for wallpaper surfaces (basic MVP).</summary>
public enum WallpaperFitMode
{
    /// <summary>UniformToFill — cover, may crop.</summary>
    Fill = 0,

    /// <summary>Fill — stretch to bounds (may distort).</summary>
    Stretch = 1,

    /// <summary>Uniform — letterbox.</summary>
    Fit = 2,
}

internal static class FitModeMapper
{
    public static Stretch ToStretch(WallpaperFitMode mode) => mode switch
    {
        WallpaperFitMode.Stretch => Stretch.Fill,
        WallpaperFitMode.Fit => Stretch.Uniform,
        _ => Stretch.UniformToFill,
    };
}

/// <summary>Video wallpaper surface using WPF MediaElement (Media Foundation).</summary>
public sealed class VideoWallpaperView : UserControl, IDisposable
{
    private readonly MediaElement _media = new()
    {
        LoadedBehavior = MediaState.Manual,
        UnloadedBehavior = MediaState.Manual,
        ScrubbingEnabled = true,
        Stretch = Stretch.UniformToFill,
        IsMuted = true,
    };

    private bool _disposed;

    public VideoWallpaperView()
    {
        Content = _media;
        Background = Brushes.Black;
        _media.MediaEnded += (_, _) =>
        {
            try
            {
                _media.Position = TimeSpan.Zero;
                _media.Play();
            }
            catch
            {
                // ignore loop errors on teardown
            }
        };
    }

    public void Apply(string path, WallpaperFitMode fit)
    {
        _media.Stretch = FitModeMapper.ToStretch(fit);
        _media.Source = new Uri(path, UriKind.Absolute);
        _media.Play();
    }

    public void Stop()
    {
        try
        {
            _media.Stop();
            _media.Close();
            _media.Source = null;
        }
        catch
        {
            // ignore
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();
    }
}

/// <summary>GIF wallpaper surface using GifBitmapDecoder + DispatcherTimer.</summary>
public sealed class GifWallpaperView : UserControl, IDisposable
{
    private readonly Image _image = new()
    {
        Stretch = Stretch.UniformToFill,
    };

    private readonly DispatcherTimer _timer = new();
    private BitmapSource[] _frames = Array.Empty<BitmapSource>();
    private int[] _delaysMs = Array.Empty<int>();
    private int _index;
    private bool _disposed;

    public GifWallpaperView()
    {
        Content = _image;
        Background = Brushes.Black;
        _timer.Tick += OnTick;
    }

    public void Apply(string path, WallpaperFitMode fit)
    {
        Stop();
        _image.Stretch = FitModeMapper.ToStretch(fit);

        var uri = new Uri(path, UriKind.Absolute);
        var decoder = new GifBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        if (decoder.Frames.Count == 0)
        {
            throw new InvalidOperationException("GIF has no frames.");
        }

        _frames = decoder.Frames.Cast<BitmapSource>().ToArray();
        _delaysMs = new int[_frames.Length];
        for (var i = 0; i < _frames.Length; i++)
        {
            _delaysMs[i] = ReadDelayMs(decoder.Frames[i]);
            if (_frames[i].CanFreeze)
            {
                _frames[i].Freeze();
            }
        }

        _index = 0;
        _image.Source = _frames[0];
        _timer.Interval = TimeSpan.FromMilliseconds(_delaysMs[0]);
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _image.Source = null;
        _frames = Array.Empty<BitmapSource>();
        _delaysMs = Array.Empty<int>();
        _index = 0;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();
        _timer.Tick -= OnTick;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_frames.Length == 0)
        {
            return;
        }

        _index = (_index + 1) % _frames.Length;
        _image.Source = _frames[_index];
        _timer.Interval = TimeSpan.FromMilliseconds(_delaysMs[_index]);
    }

    private static int ReadDelayMs(BitmapFrame frame)
    {
        // GIF delay is in hundredths of a second in the metadata query /grctlext/Delay
        const int fallback = 100;
        try
        {
            if (frame.Metadata is BitmapMetadata meta &&
                meta.GetQuery("/grctlext/Delay") is ushort delay)
            {
                var ms = delay * 10;
                return ms < 20 ? 100 : ms; // browsers treat very small delays as 100ms
            }
        }
        catch
        {
            // metadata optional
        }

        return fallback;
    }
}
