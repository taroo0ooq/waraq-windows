// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Windows;
using Waraq.Windows.Host;

namespace Waraq.Windows.Shell;

/// <summary>Phase 1a settings shell — confirms build + host probe wiring.</summary>
public partial class MainWindow : Window
{
    private readonly DesktopWallpaperHost _host = new();

    public MainWindow()
    {
        InitializeComponent();
        Title = AppInfo.ProductName;
        ProductTitle.Text = AppInfo.ProductName;
        VersionText.Text = $"Version {AppInfo.Version} · {AppInfo.License}";
        StatusText.Text = AppInfo.ScaffoldStatus;
        StrategyText.Text = $"Host strategy: {_host.StrategyName}";
        UpstreamText.Text = $"Upstream: {AppInfo.UpstreamUrl}";
        RepoText.Text = $"Repo: {AppInfo.RepoUrl}";
    }

    private void OnProbeClick(object sender, RoutedEventArgs e)
    {
        var result = _host.Probe();
        ProbeResultText.Text =
            $"{result.Message}\n" +
            $"Progman: {(result.FoundProgman ? result.ProgmanHandle.ToString("X") : "n/a")} · " +
            $"WorkerW: {(result.FoundWorkerW ? result.WorkerWHandle.ToString("X") : "n/a")}";
    }
}
