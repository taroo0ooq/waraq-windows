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
        AppWindow.Resize(new global::Windows.Graphics.SizeInt32(760, 580));

        FooterStatus.Text = _vm.ProductLine + " · Phase 4 library + profiles";
        RebuildNav();
        _navReady = true;
        SelectPane(SettingsPaneId.Library);

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
                      ?? SettingsPaneId.Library;
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
        if (id == SettingsPaneId.Library)
                {
                    ContentFrame.Content = new LibraryPaneView(
                        this,
                        ApplyPathAsync,
                        ApplyProcedural,
                        s => FooterStatus.Text = s);
                    return;
                }

        if (id == SettingsPaneId.Wallpapers)
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

    private async Task ApplyPathAsync(string path)
        {
            await App.Wallpaper.ApplyAsync(path, WallpaperFitMode.Fill);
            FooterStatus.Text =
                $"Applied ({App.Wallpaper.ActiveKind}): {App.Wallpaper.ActivePath}";
        }

        private void ApplyProcedural(string engineId)
        {
            App.Wallpaper.ApplyProcedural(engineId);
            FooterStatus.Text =
                $"Applied procedural ({App.Wallpaper.ActiveProceduralId}) via {App.Wallpaper.StrategyName}";
        }

    private async Task ApplyWallpaperAsync()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, hwnd);
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
        foreach (var ext in new[] { ".mp4", ".webm", ".mkv", ".mov", ".m4v", ".avi", ".wmv", ".gif" })
        {
            picker.FileTypeFilter.Add(ext);
        }

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            FooterStatus.Text = "Apply cancelled.";
            return;
        }

        try
        {
            await ApplyPathAsync(file.Path);
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
