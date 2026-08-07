// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Waraq.Windows.App.Tray;
using Waraq.Windows.App.Views;
using Waraq.Windows.Engines;
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

        FooterStatus.Text = _vm.ProductLine + " · Phase 3 host: Apply/Stop local video or GIF";
        RebuildNav();
        _navReady = true;
        SelectPane(SettingsPaneId.Wallpapers);

        Closed += (_, _) =>
        {
            _tray?.Dispose();
            _tray = null;
            App.ShutdownWallpaper();
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
                      ?? SettingsPaneId.Wallpapers;
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
        if (id is SettingsPaneId.Wallpapers or SettingsPaneId.Library)
        {
            var playback = new PlaybackPaneView();
            playback.Bind(desc, _vm.IsAdvancedMode, ApplyWallpaperAsync, StopWallpaper);
            ContentFrame.Content = playback;
            return;
        }

        var view = new StubPaneView();
        view.Bind(desc, _vm.IsAdvancedMode);
        ContentFrame.Content = view;
    }

    private async Task ApplyWallpaperAsync()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, hwnd);
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
        picker.FileTypeFilter.Add(".mp4");
        picker.FileTypeFilter.Add(".webm");
        picker.FileTypeFilter.Add(".mkv");
        picker.FileTypeFilter.Add(".mov");
        picker.FileTypeFilter.Add(".m4v");
        picker.FileTypeFilter.Add(".avi");
        picker.FileTypeFilter.Add(".wmv");
        picker.FileTypeFilter.Add(".gif");

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            FooterStatus.Text = "Apply cancelled.";
            return;
        }

        try
        {
            await App.Wallpaper.ApplyAsync(file.Path, WallpaperFitMode.Fill);
            FooterStatus.Text =
                $"Applied ({App.Wallpaper.ActiveKind}): {App.Wallpaper.ActivePath} · Stop to clear · exit also stops";
        }
        catch (Exception ex)
        {
            FooterStatus.Text = $"Apply failed: {ex.Message}";
        }
    }

    private void StopWallpaper()
    {
        try
        {
            App.Wallpaper.Stop();
            FooterStatus.Text = "Wallpaper stopped.";
        }
        catch (Exception ex)
        {
            FooterStatus.Text = $"Stop failed: {ex.Message}";
        }
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
