// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Waraq.Windows.Host;
using Waraq.Windows.Shell;

namespace Waraq.Windows.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = new ScaffoldViewModel();
        Title = vm.Title;
        TitleText.Text = vm.Title;
        StatusText.Text = vm.Status;
        PanesList.ItemsSource = vm.PlannedPanes;

        try
        {
            var probe = new DesktopWallpaperHost().Probe();
            ProbeText.Text =
                $"Host strategy: WorkerW (Progman) · {probe.Message} · attach deferred to Phase 3";
        }
        catch (Exception ex)
        {
            ProbeText.Text = $"Host probe skipped: {ex.Message}";
        }
    }
}
