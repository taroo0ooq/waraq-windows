// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 6: Gallery sources + privacy-first networking.

namespace Waraq.Windows.Core.Gallery;

public enum GallerySourceKind
{
    Pixabay,
    Pexels,
    Nasa,
}

public sealed class GallerySourceInfo
{
    public required GallerySourceKind Kind { get; init; }
    public required string DisplayName { get; init; }
    public required string WebsiteUrl { get; init; }
    public string? ApiKeySignupUrl { get; init; }
    public bool RequiresApiKey { get; init; }

    public static IReadOnlyList<GallerySourceInfo> All { get; } =
    [
        new()
        {
            Kind = GallerySourceKind.Pixabay,
            DisplayName = "Pixabay",
            WebsiteUrl = "https://pixabay.com/",
            ApiKeySignupUrl = "https://pixabay.com/api/docs/",
            RequiresApiKey = true,
        },
        new()
        {
            Kind = GallerySourceKind.Pexels,
            DisplayName = "Pexels",
            WebsiteUrl = "https://www.pexels.com/",
            ApiKeySignupUrl = "https://www.pexels.com/api/",
            RequiresApiKey = true,
        },
        new()
        {
            Kind = GallerySourceKind.Nasa,
            DisplayName = "NASA",
            WebsiteUrl = "https://images.nasa.gov/",
            ApiKeySignupUrl = null,
            RequiresApiKey = false,
        },
    ];

    public static GallerySourceInfo Get(GallerySourceKind kind) =>
        All.First(s => s.Kind == kind);
}

public sealed class GalleryItem
{
    public required string Id { get; init; }
    public required GallerySourceKind Source { get; init; }
    public required string Title { get; init; }
    public string? Author { get; init; }
    public string? ThumbnailUrl { get; init; }
    public required string DownloadUrl { get; init; }
    public string? PageUrl { get; init; }
    public string MediaHint { get; init; } = "video";
}

public sealed class ExternalBrowseSite
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string WebsiteUrl { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

public static class ExternalBrowseCatalog
{
    /// <summary>Links only — never scraped or proxied by Waraq.</summary>
    public static IReadOnlyList<ExternalBrowseSite> All { get; } =
    [
        new()
        {
            Id = "motionbgs",
            Name = "MotionBGs",
            Description = "Anime, gaming, and cyberpunk live wallpapers.",
            WebsiteUrl = "https://motionbgs.com/",
            Tags = ["Anime", "Gaming", "4K"],
        },
        new()
        {
            Id = "moewalls",
            Name = "MoeWalls",
            Description = "Anime live wallpaper library.",
            WebsiteUrl = "https://moewalls.com/",
            Tags = ["Anime"],
        },
        new()
        {
            Id = "mylivewallpapers",
            Name = "MyLiveWallpapers",
            Description = "Community live wallpapers.",
            WebsiteUrl = "https://mylivewallpapers.com/",
            Tags = ["Community"],
        },
        new()
        {
            Id = "wallsflow",
            Name = "Wallsflow",
            Description = "Anime, gaming, abstract live wallpapers.",
            WebsiteUrl = "https://wallsflow.com/",
            Tags = ["Anime", "Gaming"],
        },
    ];
}
