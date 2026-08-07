// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Core;

public enum MediaKind
{
    Unknown = 0,
    Video = 1,
    Gif = 2,
    Image = 3,
    Procedural = 4,
}

public static class MediaPathClassifier
{
    private static readonly HashSet<string> Video = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".mkv", ".avi", ".mov", ".wmv", ".m4v",
    };

    private static readonly HashSet<string> Gif = new(StringComparer.OrdinalIgnoreCase) { ".gif" };

    private static readonly HashSet<string> Image = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".webp",
    };

    public static MediaKind Classify(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return MediaKind.Unknown;
        }

        var ext = Path.GetExtension(path);
        if (Gif.Contains(ext)) return MediaKind.Gif;
        if (Video.Contains(ext)) return MediaKind.Video;
        if (Image.Contains(ext)) return MediaKind.Image;
        return MediaKind.Unknown;
    }
}
