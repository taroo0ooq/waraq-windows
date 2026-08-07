// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using System.Text.Json;

namespace Waraq.Windows.Core;

/// <summary>Per-display wallpaper selection keyed by stable hardware id.</summary>
public sealed class DisplayProfileStore
{
    private readonly LibraryPaths _paths;
    private readonly object _gate = new();
    private DisplayProfilesDocument _doc = new();

    public DisplayProfileStore(LibraryPaths? paths = null)
    {
        _paths = paths ?? new LibraryPaths();
        _paths.EnsureDirectories();
        Reload();
    }

    public IReadOnlyList<DisplayProfileEntry> Profiles
    {
        get
        {
            lock (_gate)
            {
                return _doc.Profiles.ToList();
            }
        }
    }

    public void Reload()
    {
        lock (_gate)
        {
            if (!File.Exists(_paths.ProfilesJson))
            {
                _doc = new DisplayProfilesDocument();
                return;
            }

            var json = File.ReadAllText(_paths.ProfilesJson);
            _doc = JsonSerializer.Deserialize<DisplayProfilesDocument>(json, LibraryJson.Options)
                   ?? new DisplayProfilesDocument();
            _doc.Profiles ??= new List<DisplayProfileEntry>();
        }
    }

    public void Save()
    {
        lock (_gate)
        {
            _paths.EnsureDirectories();
            var json = JsonSerializer.Serialize(_doc, LibraryJson.Options);
            var tmp = _paths.ProfilesJson + ".tmp";
            File.WriteAllText(tmp, json);
            File.Copy(tmp, _paths.ProfilesJson, overwrite: true);
            File.Delete(tmp);
        }
    }

    public DisplayProfileEntry Upsert(string displayKey, string? friendlyName, string? wallpaperId, WallpaperFitModeDto fit)
    {
        if (string.IsNullOrWhiteSpace(displayKey))
        {
            throw new ArgumentException("Display key is required.", nameof(displayKey));
        }

        lock (_gate)
        {
            var existing = _doc.Profiles.FirstOrDefault(p =>
                string.Equals(p.DisplayKey, displayKey, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                existing = new DisplayProfileEntry { DisplayKey = displayKey };
                _doc.Profiles.Add(existing);
            }

            existing.FriendlyName = friendlyName ?? existing.FriendlyName;
            existing.WallpaperId = wallpaperId;
            existing.Fit = fit;
            existing.UpdatedAtUtc = DateTimeOffset.UtcNow;
            Save();
            return existing;
        }
    }

    public DisplayProfileEntry? Get(string displayKey)
    {
        lock (_gate)
        {
            return _doc.Profiles.FirstOrDefault(p =>
                string.Equals(p.DisplayKey, displayKey, StringComparison.OrdinalIgnoreCase));
        }
    }
}
