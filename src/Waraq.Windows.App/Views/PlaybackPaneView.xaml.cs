// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Waraq.Windows.Shell;

namespace Waraq.Windows.App.Views;

public sealed partial class PlaybackPaneView : UserControl
{
    private Func<Task>? _applyAsync;
    private Action? _stop;

    public PlaybackPaneView()
    {
        InitializeComponent();
    }

    public void Bind(SettingsPaneDescriptor pane, bool advancedMode, Func<Task> applyAsync, Action stop)
    {
        TitleText.Text = pane.Title;
        SummaryText.Text = pane.StubSummary + " Phase 3: Apply/Stop wired.";
        _applyAsync = applyAsync;
        _stop = stop;
        StatusText.Text = App.HostRuntimeStatus();
        _ = advancedMode; // reserved for advanced host options
    }

    private async void OnApplyClick(object sender, RoutedEventArgs e)
    {
        if (_applyAsync is null)
        {
            return;
        }

        ApplyButton.IsEnabled = false;
        try
        {
            await _applyAsync();
            StatusText.Text = App.HostRuntimeStatus();
        }
        finally
        {
            ApplyButton.IsEnabled = true;
        }
    }

    private void OnStopClick(object sender, RoutedEventArgs e)
    {
        _stop?.Invoke();
        StatusText.Text = App.HostRuntimeStatus();
    }
}
