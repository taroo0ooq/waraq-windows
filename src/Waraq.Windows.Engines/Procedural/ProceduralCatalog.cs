// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 5: CPU procedural engines (mac Engines/Procedural parity set).

namespace Waraq.Windows.Engines.Procedural;

/// <summary>Renders BGRA8 premultiplied-ish frames (B,G,R,A).</summary>
public interface IProceduralEngine
{
    string Id { get; }
    string DisplayName { get; }
    string MacSource { get; }
    void Reset(int seed = 1);
    void RenderFrame(Span<byte> bgra, int width, int height, double timeSeconds);
}

public sealed record ProceduralDescriptor(string Id, string DisplayName, string MacSource);

public static class ProceduralCatalog
{
    public static IReadOnlyList<ProceduralDescriptor> All { get; } =
    [
        new("aurora", "Aurora", "Engines/Procedural/AuroraView.swift"),
        new("matrix-rain", "Matrix Rain", "Engines/Procedural/MatrixRainView.swift"),
        new("synthwave", "Synthwave", "Engines/Procedural/SynthwaveView.swift"),
        new("starfield", "Starfield", "Engines/Procedural/StarfieldView.swift"),
        new("neural-network", "Neural Network", "Engines/Procedural/NeuralNetworkView.swift"),
        new("animated-gradient", "Animated Gradient", "Engines/Procedural/ProceduralFactory.swift (+ gradient)"),
    ];

    public static IProceduralEngine Create(string id) => id.ToLowerInvariant() switch
    {
        "aurora" => new AuroraEngine(),
        "matrix-rain" or "matrixrain" => new MatrixRainEngine(),
        "synthwave" => new SynthwaveEngine(),
        "starfield" => new StarfieldEngine(),
        "neural-network" or "neuralnetwork" => new NeuralNetworkEngine(),
        "animated-gradient" or "animatedgradient" or "gradient" => new AnimatedGradientEngine(),
        _ => throw new ArgumentException($"Unknown procedural engine '{id}'.", nameof(id)),
    };

    public static bool TryCreate(string id, out IProceduralEngine? engine)
    {
        try
        {
            engine = Create(id);
            return true;
        }
        catch
        {
            engine = null;
            return false;
        }
    }
}
