// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Waraq.Windows.App.Library;
using Waraq.Windows.Core;
using Waraq.Windows.Engines.Procedural;

namespace Waraq.Windows.App.Views;

public sealed partial class LibraryPaneView : UserControl
{
    private readonly Window _owner;
    private readonly Func<string, Task> _applyPathAsync;
    private readonly Action<string> _applyProcedural;
    private readonly Action<string> _status;

    public LibraryPaneView(
        Window owner,
        Func<string, Task> applyPathAsync,
        Action<string> applyProcedural,
        Action<string> status)
    {
        _owner = owner;
        _applyPathAsync = applyPathAsync;
        _applyProcedural = applyProcedural;
        _status = status;
        InitializeComponent();
        ProceduralList.ItemsSource = ProceduralCatalog.All.ToList();
        Reload();
    }

    private void Reload()
    {
        AppServices.LibraryStore.Reload();
        var rows = AppServices.LibraryStore.Items.Select(e => new LibraryRow(e)).ToList();
        ItemsList.ItemsSource = rows;
        StatusText.Text =
            $"{rows.Count} media · {ProceduralCatalog.All.Count} procedural · {AppServices.LibraryPaths.Root}";
    }

    private async void OnImportClick(object sender, RoutedEventArgs e)
    {
        var hwnd = WindowNative.GetWindowHandle(_owner);
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, hwnd);
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
        foreach (var ext in new[] { ".mp4", ".webm", ".mkv", ".mov", ".m4v", ".avi", ".wmv", ".gif", ".png", ".jpg", ".jpeg", ".webp" })
        {
            picker.FileTypeFilter.Add(ext);
        }

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        try
        {
            var entry = AppServices.LibraryStore.Import(
                file.Path,
                (media, id) => ThumbnailFactory.TryCreate(AppServices.LibraryPaths, media, id));

            var displays = AppServices.RefreshDisplays();
            var primary = displays.FirstOrDefault();
            if (!string.IsNullOrEmpty(primary.Key))
            {
                AppServices.ProfileStore.Upsert(primary.Key, primary.FriendlyName, entry.Id, WallpaperFitModeDto.Fill);
            }

            Reload();
            _status($"Imported {entry.DisplayName}");
        }
        catch (Exception ex)
        {
            _status($"Import failed: {ex.Message}");
        }
    }

    private async void OnApplyClick(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedItem is not LibraryRow row)
        {
            _status("Select a library item first.");
            return;
        }

        try
        {
            var abs = AppServices.LibraryStore.ResolveAbsolute(row.Entry.RelativePath);
            await _applyPathAsync(abs);

            var displays = AppServices.RefreshDisplays();
            foreach (var d in displays)
            {
                AppServices.ProfileStore.Upsert(d.Key, d.FriendlyName, row.Entry.Id, WallpaperFitModeDto.Fill);
            }

            Reload();
            _status($"Applied {row.DisplayName}");
        }
        catch (Exception ex)
        {
            _status($"Apply failed: {ex.Message}");
        }
    }

    private void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedItem is not LibraryRow row)
        {
            _status("Select a library item first.");
            return;
        }

        if (AppServices.LibraryStore.Remove(row.Entry.Id))
        {
            Reload();
            _status($"Removed {row.DisplayName}");
        }
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e) => Reload();

    private void OnProceduralApplyClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string id })
        {
            try
            {
                _applyProcedural(id);
                _status($"Applied procedural: {id}");
            }
            catch (Exception ex)
            {
                _status($"Procedural apply failed: {ex.Message}");
            }
        }
    }

    private void OnProceduralDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (ProceduralList.SelectedItem is ProceduralDescriptor d)
        {
            try
            {
                _applyProcedural(d.Id);
                _status($"Applied procedural: {d.DisplayName}");
            }
            catch (Exception ex)
            {
                _status($"Procedural apply failed: {ex.Message}");
            }
        }
    }

    private sealed class LibraryRow
    {
        public LibraryRow(LibraryEntry entry)
        {
            Entry = entry;
            DisplayName = entry.DisplayName;
            Kind = entry.Kind;
            Subtitle = $"{entry.ByteLength / 1024} KiB · {entry.ImportedAtUtc:u}";
        }

        public LibraryEntry Entry { get; }
        public string DisplayName { get; }
        public string Kind { get; }
        public string Subtitle { get; }
    }
}
