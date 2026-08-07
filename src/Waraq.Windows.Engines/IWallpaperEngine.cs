// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Waraq.Windows.Core;
using Waraq.Windows.Engines.Procedural;

namespace Waraq.Windows.Engines;

public enum WallpaperFitMode
{
    Fill = 0,
    Stretch = 1,
    Fit = 2,
}

public interface IWallpaperEngine
{
    MediaKind Kind { get; }
    string DisplayName { get; }
}

public static class EngineCatalog
{
    public static IReadOnlyList<string> PlannedEngines { get; } =
    [
        "Video",
        "Gif",
        "Image",
        "Procedural.Aurora",
        "Procedural.MatrixRain",
        "Procedural.Synthwave",
        "Procedural.Starfield",
        "Procedural.NeuralNetwork",
        "Procedural.AnimatedGradient",
    ];

    public static bool IsPhase3Playable(MediaKind kind) =>
        kind is MediaKind.Video or MediaKind.Gif;

    public static IReadOnlyList<ProceduralDescriptor> ProceduralEngines => ProceduralCatalog.All;
}
