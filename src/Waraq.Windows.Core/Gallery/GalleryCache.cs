// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using System.Text.Json;

namespace Waraq.Windows.Core.Gallery;

/// <summary>24h response cache (Pixabay terms). Memory + disk under AppData.</summary>
public sealed class GalleryCache
{
    private readonly string _dir;
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    public GalleryCache(string? cacheDir = null)
    {
        _dir = cacheDir ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Waraq", "GalleryCache");
        Directory.CreateDirectory(_dir);
    }

    public IReadOnlyList<GalleryItem>? TryGet(GallerySourceKind source, string query)
    {
        var path = PathFor(source, query);
        if (!File.Exists(path))
        {
            return null;
        }

        var info = new FileInfo(path);
        if (DateTime.UtcNow - info.LastWriteTimeUtc > Ttl)
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<GalleryItem>>(json);
        }
        catch
        {
            return null;
        }
    }

    public void Store(GallerySourceKind source, string query, IReadOnlyList<GalleryItem> items)
    {
        var path = PathFor(source, query);
        var json = JsonSerializer.Serialize(items);
        File.WriteAllText(path, json);
    }

    private string PathFor(GallerySourceKind source, string query)
    {
        var safe = string.Join("_", query.ToLowerInvariant().Split(Path.GetInvalidFileNameChars()));
        if (safe.Length > 40)
        {
            safe = safe[..40];
        }

        return Path.Combine(_dir, $"{source}_{safe}.json");
    }
}
