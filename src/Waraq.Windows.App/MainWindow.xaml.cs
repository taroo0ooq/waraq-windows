// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Waraq.Windows.App.Tray;
using Waraq.Windows.App.Views;
using Waraq.Windows.Shell;

namespace Waraq.Windows.App;

public sealed partial class MainWindow : Window
{
    private readonly SettingsShellViewModel _vm = new();
    private TrayIconService? _tray;
    private bool _navReady;

    public MainWindow()
    {
        InitializeComponent();
        Title = _vm.WindowTitle;

        AppWindow.Resize(new global::Windows.Graphics.SizeInt32(720, 560));

        FooterStatus.Text = _vm.ProductLine + " · Phase 2 design shell stubs · owner visual accept later";
        RebuildNav();
        _navReady = true;
        SelectPane(SettingsPaneId.General);

        Closed += (_, _) =>
        {
            _tray?.Dispose();
            _tray = null;
        };

        try
        {
            _tray = new TrayIconService(this);
        }
        catch (Exception ex)
        {
            FooterStatus.Text += $" · tray unavailable: {ex.Message}";
        }
    }

    private void RebuildNav()
    {
        NavView.MenuItems.Clear();
        foreach (var pane in _vm.VisiblePanes)
        {
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = pane.Title,
                Tag = pane.Id.ToString(),
                Icon = new FontIcon
                {
                    Glyph = pane.Glyph,
                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe Fluent Icons"),
                },
            });
        }
    }

    private void AdvancedToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (!_navReady)
        {
            return;
        }

        _vm.IsAdvancedMode = AdvancedToggle.IsOn;
        var current = ParsePaneTag((NavView.SelectedItem as NavigationViewItem)?.Tag)
                      ?? SettingsPaneId.General;
        RebuildNav();
        if (!_vm.IsAdvancedMode && current == SettingsPaneId.Diagnostics)
        {
            current = SettingsPaneId.General;
        }

        SelectPane(current);
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var id = ParsePaneTag(item.Tag);
            if (id is not null)
            {
                ShowPane(id.Value);
            }
        }
    }

    private void SelectPane(SettingsPaneId id)
    {
        foreach (var item in NavView.MenuItems.OfType<NavigationViewItem>())
        {
            if (ParsePaneTag(item.Tag) == id)
            {
                NavView.SelectedItem = item;
                ShowPane(id);
                return;
            }
        }
    }

    private void ShowPane(SettingsPaneId id)
    {
        var desc = SettingsNavCatalog.All.First(p => p.Id == id);
        var view = new StubPaneView();
        view.Bind(desc, _vm.IsAdvancedMode);
        ContentFrame.Content = view;
    }

    private static SettingsPaneId? ParsePaneTag(object? tag)
    {
        if (tag is string s && Enum.TryParse<SettingsPaneId>(s, out var id))
        {
            return id;
        }

        return null;
    }
}
