// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Waraq.Windows.Core;

public enum WallpaperFitModeDto
{
    Fill = 0,
    Stretch = 1,
    Fit = 2,
}

public sealed class LibraryEntry
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string RelativePath { get; set; } = "";
    public string? ThumbnailRelativePath { get; set; }
    public string Kind { get; set; } = "Unknown";
    public long ByteLength { get; set; }
    public DateTimeOffset ImportedAtUtc { get; set; }
    public string SourcePathHint { get; set; } = "";
}

public sealed class LibraryDocument
{
    public int Version { get; set; } = 1;
    public List<LibraryEntry> Items { get; set; } = new();
}

public sealed class DisplayProfileEntry
{
    /// <summary>Stable hardware key (PNP/DeviceID), not volatile GDI index.</summary>
    public string DisplayKey { get; set; } = "";
    public string? FriendlyName { get; set; }
    public string? WallpaperId { get; set; }
    public WallpaperFitModeDto Fit { get; set; } = WallpaperFitModeDto.Fill;
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class DisplayProfilesDocument
{
    public int Version { get; set; } = 1;
    public List<DisplayProfileEntry> Profiles { get; set; } = new();
}

internal static class LibraryJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}
