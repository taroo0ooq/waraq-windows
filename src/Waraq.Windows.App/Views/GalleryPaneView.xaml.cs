// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Waraq.Windows.Core.Gallery;

namespace Waraq.Windows.App.Views;

public sealed partial class GalleryPaneView : UserControl
{
    private readonly Action<string> _status;
    private readonly Func<string, Task> _importLocalPathAsync;
    private IReadOnlyList<GalleryItem> _results = Array.Empty<GalleryItem>();

    public GalleryPaneView(Action<string> status, Func<string, Task>? importLocalPathAsync = null)
    {
        _status = status;
        _importLocalPathAsync = importLocalPathAsync ?? (_ => Task.CompletedTask);
        InitializeComponent();

        SourceBox.ItemsSource = GallerySourceInfo.All.Select(s => s.DisplayName).ToList();
        SourceBox.SelectedIndex = 0;
        BrowseList.ItemsSource = ExternalBrowseCatalog.All.ToList();
        RefreshKeyUi();
        StatusText.Text = "Idle — no network until Search.";
    }

    private GallerySourceKind CurrentSource
    {
        get
        {
            var name = SourceBox.SelectedItem as string ?? "NASA";
            return GallerySourceInfo.All.First(s => s.DisplayName == name).Kind;
        }
    }

    private void OnSourceChanged(object sender, SelectionChangedEventArgs e) => RefreshKeyUi();

    private void RefreshKeyUi()
    {
        var info = GallerySourceInfo.Get(CurrentSource);
        KeyPanel.Visibility = info.RequiresApiKey ? Visibility.Visible : Visibility.Collapsed;
        KeyBox.Text = AppServices.ApiKeys.GetKey(CurrentSource) ?? "";
    }

    private void OnSaveKey(object sender, RoutedEventArgs e)
    {
        AppServices.ApiKeys.SetKey(CurrentSource, KeyBox.Text);
        _status($"API key saved locally for {CurrentSource}.");
        StatusText.Text = $"Key saved for {CurrentSource} (AppData\\Waraq\\gallery-keys.json).";
    }

    private async void OnSearchClick(object sender, RoutedEventArgs e)
    {
        var q = QueryBox.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(q))
        {
            StatusText.Text = "Enter a search query first.";
            return;
        }

        var info = GallerySourceInfo.Get(CurrentSource);
        if (info.RequiresApiKey && !AppServices.ApiKeys.HasKey(CurrentSource))
        {
            StatusText.Text = $"{info.DisplayName} requires an API key. Save one first.";
            return;
        }

        SearchButton.IsEnabled = false;
        StatusText.Text = "Searching (network allowed for this action)…";
        try
        {
            _results = await AppServices.GallerySearch.SearchAsync(CurrentSource, q);
            ResultsList.ItemsSource = _results.Select(i => new ResultRow(i)).ToList();
            StatusText.Text =
                $"{_results.Count} result(s) · network calls this session: {AppServices.GallerySearch.NetworkCallCount}";
            _status($"Gallery search: {_results.Count} hits from {info.DisplayName}");
        }
        catch (Exception ex)
        {
            StatusText.Text = "Search failed: " + ex.Message;
            _status("Gallery search failed: " + ex.Message);
        }
        finally
        {
            SearchButton.IsEnabled = true;
        }
    }

    private async void OnImportClick(object sender, RoutedEventArgs e)
    {
        if (ResultsList.SelectedItem is not ResultRow row)
        {
            StatusText.Text = "Select a result first.";
            return;
        }

        try
        {
            StatusText.Text = "Downloading to temp (user-initiated, https-only)…";
            var path = await DownloadToTempAsync(row.Item).ConfigureAwait(true);
            var entry = AppServices.LibraryStore.Import(path);
            try { File.Delete(path); } catch { /* ok */ }
            StatusText.Text = $"Imported to library: {entry.DisplayName}";
            _status($"Imported gallery item {entry.DisplayName}");
            await _importLocalPathAsync(AppServices.LibraryStore.ResolveAbsolute(entry.RelativePath));
        }
        catch (Exception ex)
        {
            StatusText.Text = "Import failed: " + ex.Message;
        }
    }

    private void OnOpenItemPage(object sender, RoutedEventArgs e)
    {
        if (ResultsList.SelectedItem is not ResultRow row || string.IsNullOrWhiteSpace(row.Item.PageUrl))
        {
            StatusText.Text = "No page URL on selection.";
            return;
        }

        try
        {
            BrowseWeb.Open(row.Item.PageUrl!);
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
    }

    private void OnBrowseOpen(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string url })
        {
            try
            {
                BrowseWeb.Open(url);
                StatusText.Text = "Opened in default browser (no scrape).";
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }
    }

    private static async Task<string> DownloadToTempAsync(GalleryItem item)
    {
        GalleryUrlPolicy.EnsureSafeHttpsUrl(item.DownloadUrl, "Gallery import");

        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        var ext = ".mp4";
        if (item.DownloadUrl.Contains(".webm", StringComparison.OrdinalIgnoreCase))
        {
            ext = ".webm";
        }

        var path = Path.Combine(Path.GetTempPath(), "waraq-gallery-" + Guid.NewGuid().ToString("N") + ext);
        try
        {
            await GalleryUrlPolicy.DownloadToFileAsync(http, item.DownloadUrl, path).ConfigureAwait(false);
            return path;
        }
        catch
        {
            try { File.Delete(path); } catch { /* ok */ }
            throw;
        }
    }

    private sealed class ResultRow
    {
        public ResultRow(GalleryItem item)
        {
            Item = item;
            Title = item.Title;
            Subtitle = $"{item.Source} · {item.Author ?? "—"}";
        }

        public GalleryItem Item { get; }
        public string Title { get; }
        public string Subtitle { get; }
    }
}
