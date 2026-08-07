// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 4: local library under AppData\Waraq.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Waraq.Windows.Core;

public sealed class WallpaperLibraryStore
{
    private readonly LibraryPaths _paths;
    private readonly object _gate = new();
    private LibraryDocument _doc = new();

    public WallpaperLibraryStore(LibraryPaths? paths = null)
    {
        _paths = paths ?? new LibraryPaths();
        _paths.EnsureDirectories();
        Reload();
    }

    public LibraryPaths Paths => _paths;

    public IReadOnlyList<LibraryEntry> Items
    {
        get
        {
            lock (_gate)
            {
                return _doc.Items.OrderByDescending(i => i.ImportedAtUtc).ToList();
            }
        }
    }

    public void Reload()
    {
        lock (_gate)
        {
            if (!File.Exists(_paths.LibraryJson))
            {
                _doc = new LibraryDocument();
                return;
            }

            var json = File.ReadAllText(_paths.LibraryJson);
            _doc = JsonSerializer.Deserialize<LibraryDocument>(json, LibraryJson.Options) ?? new LibraryDocument();
            _doc.Items ??= new List<LibraryEntry>();
        }
    }

    public void Save()
    {
        lock (_gate)
        {
            _paths.EnsureDirectories();
            var json = JsonSerializer.Serialize(_doc, LibraryJson.Options);
            var tmp = _paths.LibraryJson + ".tmp";
            File.WriteAllText(tmp, json);
            File.Copy(tmp, _paths.LibraryJson, overwrite: true);
            File.Delete(tmp);
        }
    }

    /// <summary>Import a local media file into the library (copy + optional thumbnail).</summary>
    public LibraryEntry Import(string sourcePath, Func<string, string, string?>? thumbnailFactory = null)
    {
        var full = LocalMediaPathGate.NormalizeExistingLocalFile(sourcePath);
        var kind = MediaPathClassifier.Classify(full);
        if (kind is not (MediaKind.Video or MediaKind.Gif or MediaKind.Image))
        {
            throw new NotSupportedException("Library import supports video, GIF, and image files.");
        }

        LocalMediaPathGate.EnsureWithinSizeLimit(full, kind == MediaKind.Image ? MediaKind.Gif : kind);

        var ext = Path.GetExtension(full);
        if (string.IsNullOrEmpty(ext))
        {
            ext = kind == MediaKind.Gif ? ".gif" : ".bin";
        }

        var id = MakeId(full);
        var fileName = id + ext.ToLowerInvariant();
        var dest = Path.Combine(_paths.WallpapersDir, fileName);
        File.Copy(full, dest, overwrite: true);

        string? thumbRel = null;
        try
        {
            var thumbAbs = thumbnailFactory?.Invoke(dest, id);
            if (!string.IsNullOrWhiteSpace(thumbAbs) && File.Exists(thumbAbs))
            {
                thumbRel = Path.GetRelativePath(_paths.Root, thumbAbs);
            }
        }
        catch
        {
            // thumbnail is best-effort
        }

        var entry = new LibraryEntry
        {
            Id = id,
            DisplayName = Path.GetFileName(full),
            RelativePath = Path.Combine("Wallpapers", fileName),
            ThumbnailRelativePath = thumbRel,
            Kind = kind.ToString(),
            ByteLength = new FileInfo(dest).Length,
            ImportedAtUtc = DateTimeOffset.UtcNow,
            SourcePathHint = full,
        };

        lock (_gate)
        {
            _doc.Items.RemoveAll(i => i.Id == id);
            _doc.Items.Add(entry);
        }

        Save();
        return entry;
    }

    public bool Remove(string id)
    {
        LibraryEntry? removed;
        lock (_gate)
        {
            removed = _doc.Items.FirstOrDefault(i => i.Id == id);
            if (removed is null)
            {
                return false;
            }

            _doc.Items.Remove(removed);
        }

        TryDelete(ResolveAbsolute(removed.RelativePath));
        if (!string.IsNullOrWhiteSpace(removed.ThumbnailRelativePath))
        {
            TryDelete(ResolveAbsolute(removed.ThumbnailRelativePath!));
        }

        Save();
        return true;
    }

    public string ResolveAbsolute(string relativeOrAbsolute)
    {
        if (Path.IsPathRooted(relativeOrAbsolute))
        {
            return Path.GetFullPath(relativeOrAbsolute);
        }

        return Path.GetFullPath(Path.Combine(_paths.Root, relativeOrAbsolute));
    }

    public LibraryEntry? Find(string id)
    {
        lock (_gate)
        {
            return _doc.Items.FirstOrDefault(i => i.Id == id);
        }
    }

    private static string MakeId(string fullPath)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(fullPath.ToUpperInvariant()));
        return Convert.ToHexString(bytes.AsSpan(0, 8)).ToLowerInvariant();
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // best-effort
        }
    }
}
