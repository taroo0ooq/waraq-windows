// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Core;

/// <summary>%AppData%\Waraq layout (mac parity: Application Support/Waraq).</summary>
public sealed class LibraryPaths
{
    public string Root { get; }
    public string WallpapersDir => Path.Combine(Root, "Wallpapers");
    public string ThumbnailsDir => Path.Combine(Root, "Thumbnails");
    public string LibraryJson => Path.Combine(Root, "library.json");
    public string ProfilesJson => Path.Combine(Root, "profiles.json");

    public LibraryPaths(string? root = null)
    {
        Root = string.IsNullOrWhiteSpace(root)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Waraq")
            : Path.GetFullPath(root);
    }

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(WallpapersDir);
        Directory.CreateDirectory(ThumbnailsDir);
    }
}
