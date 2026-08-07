// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Network ONLY when SearchAsync is invoked (explicit user action).

using System.Net.Http.Headers;
using System.Text.Json;

namespace Waraq.Windows.Core.Gallery;

public interface IGalleryClient
{
    GallerySourceKind Source { get; }
    Task<IReadOnlyList<GalleryItem>> SearchAsync(string query, CancellationToken ct = default);
}

public sealed class GallerySearchService
{
    private readonly ApiKeyStore _keys;
    private readonly GalleryCache _cache;
    private readonly HttpClient _http;
    private long _networkCalls;

    public GallerySearchService(ApiKeyStore keys, GalleryCache cache, HttpClient? http = null)
    {
        _keys = keys;
        _cache = cache;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    /// <summary>Observability for tests — increments only on real HTTP.</summary>
    public long NetworkCallCount => Interlocked.Read(ref _networkCalls);

    public Task<IReadOnlyList<GalleryItem>> SearchAsync(
        GallerySourceKind source,
        string query,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Search query is required.", nameof(query));
        }

        query = query.Trim();
        var cached = _cache.TryGet(source, query);
        if (cached is not null)
        {
            return Task.FromResult(cached);
        }

        IGalleryClient client = source switch
        {
            GallerySourceKind.Pixabay => new PixabayClient(_keys, _http, () => Interlocked.Increment(ref _networkCalls)),
            GallerySourceKind.Pexels => new PexelsClient(_keys, _http, () => Interlocked.Increment(ref _networkCalls)),
            GallerySourceKind.Nasa => new NasaClient(_http, () => Interlocked.Increment(ref _networkCalls)),
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };

        return SearchAndCacheAsync(client, query, ct);
    }

    private async Task<IReadOnlyList<GalleryItem>> SearchAndCacheAsync(
        IGalleryClient client, string query, CancellationToken ct)
    {
        var items = await client.SearchAsync(query, ct).ConfigureAwait(false);
        _cache.Store(client.Source, query, items);
        return items;
    }
}

internal sealed class PixabayClient : IGalleryClient
{
    private readonly ApiKeyStore _keys;
    private readonly HttpClient _http;
    private readonly Action _onNetwork;

    public PixabayClient(ApiKeyStore keys, HttpClient http, Action onNetwork)
    {
        _keys = keys;
        _http = http;
        _onNetwork = onNetwork;
    }

    public GallerySourceKind Source => GallerySourceKind.Pixabay;

    public async Task<IReadOnlyList<GalleryItem>> SearchAsync(string query, CancellationToken ct = default)
    {
        var key = _keys.GetKey(GallerySourceKind.Pixabay)
                  ?? throw new InvalidOperationException("Pixabay API key is missing.");

        var url =
            "https://pixabay.com/api/videos/?key=" + Uri.EscapeDataString(key) +
            "&q=" + Uri.EscapeDataString(query) +
            "&per_page=20&safesearch=true&video_type=film";

        _onNetwork();
        using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Pixabay HTTP {(int)resp.StatusCode}");
        }

        using var doc = JsonDocument.Parse(body);
        var hits = doc.RootElement.GetProperty("hits");
        var list = new List<GalleryItem>();
        foreach (var hit in hits.EnumerateArray())
        {
            var id = hit.GetProperty("id").GetRawText();
            var page = hit.TryGetProperty("pageURL", out var p) ? p.GetString() : null;
            string? download = null;
            string? thumb = null;
            if (hit.TryGetProperty("videos", out var videos))
            {
                foreach (var quality in new[] { "medium", "small", "large", "tiny" })
                {
                    if (videos.TryGetProperty(quality, out var v) &&
                        v.TryGetProperty("url", out var u))
                    {
                        download = u.GetString();
                        break;
                    }
                }
            }

            if (hit.TryGetProperty("userImageURL", out var ti))
            {
                thumb = ti.GetString();
            }

            if (string.IsNullOrWhiteSpace(download))
            {
                continue;
            }

            var user = hit.TryGetProperty("user", out var us) ? us.GetString() : null;
            var tags = hit.TryGetProperty("tags", out var tg) ? tg.GetString() : "Pixabay video";
            list.Add(new GalleryItem
            {
                Id = "pixabay-" + id,
                Source = GallerySourceKind.Pixabay,
                Title = tags ?? "Pixabay video",
                Author = user,
                ThumbnailUrl = thumb,
                DownloadUrl = download!,
                PageUrl = page,
            });
        }

        return list;
    }
}

internal sealed class PexelsClient : IGalleryClient
{
    private readonly ApiKeyStore _keys;
    private readonly HttpClient _http;
    private readonly Action _onNetwork;

    public PexelsClient(ApiKeyStore keys, HttpClient http, Action onNetwork)
    {
        _keys = keys;
        _http = http;
        _onNetwork = onNetwork;
    }

    public GallerySourceKind Source => GallerySourceKind.Pexels;

    public async Task<IReadOnlyList<GalleryItem>> SearchAsync(string query, CancellationToken ct = default)
    {
        var key = _keys.GetKey(GallerySourceKind.Pexels)
                  ?? throw new InvalidOperationException("Pexels API key is missing.");

        var url = "https://api.pexels.com/videos/search?query=" + Uri.EscapeDataString(query) +
                  "&per_page=20";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue(key);
        // Pexels expects Authorization: YOUR_API_KEY (not Bearer)
        req.Headers.Remove("Authorization");
        req.Headers.TryAddWithoutValidation("Authorization", key);

        _onNetwork();
        using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Pexels HTTP {(int)resp.StatusCode}");
        }

        using var doc = JsonDocument.Parse(body);
        var videos = doc.RootElement.GetProperty("videos");
        var list = new List<GalleryItem>();
        foreach (var v in videos.EnumerateArray())
        {
            var id = v.GetProperty("id").GetRawText();
            var user = v.TryGetProperty("user", out var u) && u.TryGetProperty("name", out var n)
                ? n.GetString()
                : null;
            var image = v.TryGetProperty("image", out var im) ? im.GetString() : null;
            var page = v.TryGetProperty("url", out var pu) ? pu.GetString() : null;
            string? download = null;
            if (v.TryGetProperty("video_files", out var files))
            {
                // prefer hd mp4
                foreach (var f in files.EnumerateArray())
                {
                    var link = f.TryGetProperty("link", out var l) ? l.GetString() : null;
                    var fileType = f.TryGetProperty("file_type", out var ft) ? ft.GetString() : null;
                    if (link is null) continue;
                    if (fileType is null || fileType.Contains("mp4", StringComparison.OrdinalIgnoreCase))
                    {
                        download = link;
                        if (f.TryGetProperty("quality", out var q) &&
                            string.Equals(q.GetString(), "hd", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(download))
            {
                continue;
            }

            list.Add(new GalleryItem
            {
                Id = "pexels-" + id,
                Source = GallerySourceKind.Pexels,
                Title = "Pexels video " + id,
                Author = user,
                ThumbnailUrl = image,
                DownloadUrl = download!,
                PageUrl = page,
            });
        }

        return list;
    }
}

internal sealed class NasaClient : IGalleryClient
{
    private readonly HttpClient _http;
    private readonly Action _onNetwork;

    public NasaClient(HttpClient http, Action onNetwork)
    {
        _http = http;
        _onNetwork = onNetwork;
    }

    public GallerySourceKind Source => GallerySourceKind.Nasa;

    public async Task<IReadOnlyList<GalleryItem>> SearchAsync(string query, CancellationToken ct = default)
    {
        var url = "https://images-api.nasa.gov/search?q=" + Uri.EscapeDataString(query) +
                  "&media_type=video";

        _onNetwork();
        using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"NASA HTTP {(int)resp.StatusCode}");
        }

        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("collection").GetProperty("items");
        var list = new List<GalleryItem>();
        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("data", out var dataArr) || dataArr.GetArrayLength() == 0)
            {
                continue;
            }

            var data = dataArr[0];
            var nasaId = data.TryGetProperty("nasa_id", out var nid) ? nid.GetString() : null;
            if (string.IsNullOrWhiteSpace(nasaId))
            {
                continue;
            }

            var title = data.TryGetProperty("title", out var t) ? t.GetString() : nasaId;
            string? thumb = null;
            if (item.TryGetProperty("links", out var links))
            {
                foreach (var link in links.EnumerateArray())
                {
                    if (link.TryGetProperty("rel", out var rel) &&
                        rel.GetString() == "preview" &&
                        link.TryGetProperty("href", out var href))
                    {
                        thumb = href.GetString();
                        break;
                    }
                }
            }

            var pathId = string.Join("/", nasaId.Split('/').Select(Uri.EscapeDataString));
            var download = $"https://images-assets.nasa.gov/video/{pathId}/{pathId}~medium.mp4";

            list.Add(new GalleryItem
            {
                Id = "nasa-" + nasaId,
                Source = GallerySourceKind.Nasa,
                Title = title ?? nasaId,
                Author = "NASA",
                ThumbnailUrl = thumb,
                DownloadUrl = download,
                PageUrl = "https://images.nasa.gov/details/" + Uri.EscapeDataString(nasaId),
            });
        }

        return list;
    }
}

/// <summary>Browse Web — open default browser only. Never fetch site content.</summary>
public static class BrowseWeb
{
    public static void Open(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL is required.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Only http(s) URLs can be opened.", nameof(url));
        }

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = uri.AbsoluteUri,
            UseShellExecute = true,
        };
        System.Diagnostics.Process.Start(psi);
    }
}
