// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 1: engine contracts only. Implementations in Phase 3–5.

using Waraq.Windows.Core;

namespace Waraq.Windows.Engines;

public interface IWallpaperEngine
{
    MediaKind Kind { get; }
    string DisplayName { get; }
}

/// <summary>Placeholder registry — Phase 3+ registers real engines.</summary>
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
}
