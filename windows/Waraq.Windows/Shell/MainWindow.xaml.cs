// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Windows;
using Microsoft.Win32;
using Waraq.Windows.Engines;

namespace Waraq.Windows.Shell;

/// <summary>MVP settings shell: pick local media, apply/stop WorkerW wallpaper.</summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = AppInfo.ProductName;
        ProductTitle.Text = AppInfo.ProductName;
        VersionText.Text = $"Version {AppInfo.Version} · {AppInfo.License}";
        StrategyText.Text = $"Host: {App.Wallpaper.StrategyName} · {AppInfo.StatusLine}";
        RefreshStatus("Ready.");
    }

    private void OnBrowseClick(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Choose wallpaper media",
            Filter =
                "Wallpaper media|*.mp4;*.webm;*.mkv;*.avi;*.mov;*.wmv;*.m4v;*.gif|" +
                "Video|*.mp4;*.webm;*.mkv;*.avi;*.mov;*.wmv;*.m4v|" +
                "GIF|*.gif|" +
                "All files|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };

        if (dlg.ShowDialog(this) == true)
        {
            PathBox.Text = dlg.FileName;
        }
    }

    private void OnApplyClick(object sender, RoutedEventArgs e)
    {
        var path = PathBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            RefreshStatus("Choose a media file first.");
            return;
        }

        try
        {
            var fit = FitBox.SelectedIndex switch
            {
                1 => WallpaperFitMode.Stretch,
                2 => WallpaperFitMode.Fit,
                _ => WallpaperFitMode.Fill,
            };

            App.Wallpaper.Apply(path, fit);
            RefreshStatus(
                $"Applied ({App.Wallpaper.ActiveKind}): {App.Wallpaper.ActivePath}\n" +
                $"Fit: {fit}\n" +
                "Icons should remain clickable. Stop clears the surface. Exiting the app also stops wallpaper.\n" +
                "Note: battery/fullscreen pause is deferred (follow-up). Tested target: Windows 10/11.");
        }
        catch (Exception ex)
        {
            RefreshStatus($"Apply failed: {ex.Message}");
        }
    }

    private void OnStopClick(object sender, RoutedEventArgs e)
    {
        try
        {
            App.Wallpaper.Stop();
            RefreshStatus("Stopped. Wallpaper surface closed.");
        }
        catch (Exception ex)
        {
            RefreshStatus($"Stop failed: {ex.Message}");
        }
    }

    private void OnProbeClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = App.Wallpaper.Probe();
            RefreshStatus(
                $"{result.Message}\n" +
                $"Progman: {(result.FoundProgman ? result.ProgmanHandle.ToString("X") : "n/a")} · " +
                $"WorkerW: {(result.FoundWorkerW ? result.WorkerWHandle.ToString("X") : "n/a")}\n" +
                $"Running: {App.Wallpaper.IsRunning}");
        }
        catch (Exception ex)
        {
            RefreshStatus($"Probe failed: {ex.Message}");
        }
    }

    private void RefreshStatus(string message)
    {
        StatusText.Text = message;
    }
}
