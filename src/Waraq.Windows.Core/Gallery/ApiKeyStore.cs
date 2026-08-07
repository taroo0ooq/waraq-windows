// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Local API keys only — never phone-home.

using System.Text.Json;

namespace Waraq.Windows.Core.Gallery;

public sealed class ApiKeyStore
{
    private readonly string _path;
    private readonly object _gate = new();
    private Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase);

    public ApiKeyStore(string? storePath = null)
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Waraq");
        Directory.CreateDirectory(root);
        _path = storePath ?? Path.Combine(root, "gallery-keys.json");
        Reload();
    }

    public string StorePath => _path;

    public void Reload()
    {
        lock (_gate)
        {
            if (!File.Exists(_path))
            {
                _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            try
            {
                var json = File.ReadAllText(_path);
                _map = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    public string? GetKey(GallerySourceKind source)
    {
        lock (_gate)
        {
            if (!_map.TryGetValue(source.ToString(), out var raw))
            {
                return null;
            }

            var trimmed = raw.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }
    }

    public void SetKey(GallerySourceKind source, string? key)
    {
        lock (_gate)
        {
            var trimmed = key?.Trim() ?? "";
            if (string.IsNullOrEmpty(trimmed))
            {
                _map.Remove(source.ToString());
            }
            else
            {
                _map[source.ToString()] = trimmed;
            }

            Save();
        }
    }

    public bool HasKey(GallerySourceKind source) => GetKey(source) is not null;

    private void Save()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(_map, new JsonSerializerOptions { WriteIndented = true });
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, json);

        // Clear Hidden before replace — otherwise File.Copy can throw UnauthorizedAccessException.
        try
        {
            if (File.Exists(_path))
            {
                var existing = File.GetAttributes(_path);
                if ((existing & FileAttributes.Hidden) != 0)
                {
                    File.SetAttributes(_path, existing & ~FileAttributes.Hidden);
                }
            }
        }
        catch
        {
            // non-fatal
        }

        File.Copy(tmp, _path, overwrite: true);
        File.Delete(tmp);

        // Best-effort: mark keys file as user-hidden (does not encrypt; residual risk documented).
        try
        {
            var attrs = File.GetAttributes(_path);
            File.SetAttributes(_path, attrs | FileAttributes.Hidden);
        }
        catch
        {
            // non-fatal
        }
    }
}
