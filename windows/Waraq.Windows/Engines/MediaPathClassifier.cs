// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Waraq.Windows.Engines;

public enum MediaKind
{
    Unknown = 0,
    Video = 1,
    Gif = 2,
}

/// <summary>Classifies local media paths for the MVP engines.</summary>
public static class MediaPathClassifier
{
    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".mkv", ".avi", ".mov", ".wmv", ".m4v",
    };

    private static readonly HashSet<string> GifExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".gif",
    };

    public static MediaKind Classify(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return MediaKind.Unknown;
        }

        var ext = System.IO.Path.GetExtension(path);
        if (GifExtensions.Contains(ext))
        {
            return MediaKind.Gif;
        }

        if (VideoExtensions.Contains(ext))
        {
            return MediaKind.Video;
        }

        return MediaKind.Unknown;
    }

    public static bool IsSupported(string? path) => Classify(path) != MediaKind.Unknown;
}
