// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Waraq.Windows.Shell;

namespace Waraq.Windows.App.Views;

public sealed partial class StubPaneView : UserControl
{
    public StubPaneView()
    {
        InitializeComponent();
    }

    public void Bind(SettingsPaneDescriptor pane, bool advancedMode)
    {
        TitleText.Text = pane.Title;
        SummaryText.Text = pane.StubSummary;
        MacRefText.Text = $"macOS reference: {pane.MacScreenshotRef}";
        AdvancedPill.Visibility = advancedMode
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
