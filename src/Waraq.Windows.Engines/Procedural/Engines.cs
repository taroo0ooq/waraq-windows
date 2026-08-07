// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Engines.Procedural;

internal static class ProcColor
{
    public static void Set(Span<byte> bgra, int i, byte b, byte g, byte r, byte a = 255)
    {
        var o = i * 4;
        bgra[o] = b;
        bgra[o + 1] = g;
        bgra[o + 2] = r;
        bgra[o + 3] = a;
    }

    public static byte Clamp(double v) => (byte)Math.Clamp((int)v, 0, 255);

    public static void Fill(Span<byte> bgra, byte b, byte g, byte r)
    {
        for (var i = 0; i < bgra.Length / 4; i++)
        {
            Set(bgra, i, b, g, r);
        }
    }
}

/// <summary>Soft green/cyan curtains (mac Aurora).</summary>
public sealed class AuroraEngine : IProceduralEngine
{
    public string Id => "aurora";
    public string DisplayName => "Aurora";
    public string MacSource => "Engines/Procedural/AuroraView.swift";
    public void Reset(int seed = 1) { }

    public void RenderFrame(Span<byte> bgra, int width, int height, double t)
    {
        for (var y = 0; y < height; y++)
        {
            var ny = y / (double)height;
            for (var x = 0; x < width; x++)
            {
                var nx = x / (double)width;
                var wave = Math.Sin(nx * 6 + t * 0.8) * 0.25
                           + Math.Sin(nx * 3 - t * 0.5 + ny * 4) * 0.2
                           + Math.Sin((nx + ny) * 5 + t * 0.3) * 0.15;
                var v = 0.25 + 0.55 * (0.5 + 0.5 * Math.Sin(ny * 3 + wave * 4 + t * 0.2));
                var g = ProcColor.Clamp(40 + v * 180);
                var b = ProcColor.Clamp(80 + v * 140);
                var r = ProcColor.Clamp(10 + v * 40);
                ProcColor.Set(bgra, y * width + x, b, g, r);
            }
        }
    }
}

/// <summary>Falling code columns (mac Matrix Rain).</summary>
public sealed class MatrixRainEngine : IProceduralEngine
{
    private readonly List<float> _offsets = new();
    private int _cols;

    public string Id => "matrix-rain";
    public string DisplayName => "Matrix Rain";
    public string MacSource => "Engines/Procedural/MatrixRainView.swift";

    public void Reset(int seed = 1)
    {
        _offsets.Clear();
        _cols = 0;
    }

    public void RenderFrame(Span<byte> bgra, int width, int height, double t)
    {
        var colW = Math.Max(6, width / 80);
        var cols = Math.Max(1, width / colW);
        if (cols != _cols)
        {
            _cols = cols;
            _offsets.Clear();
            var rng = new Random(42);
            for (var i = 0; i < cols; i++)
            {
                _offsets.Add((float)rng.NextDouble() * height);
            }
        }

        ProcColor.Fill(bgra, 0, 8, 0);
        for (var c = 0; c < cols; c++)
        {
            var head = (int)((_offsets[c] + t * (40 + c % 7) * 12) % (height + 40));
            for (var trail = 0; trail < 18; trail++)
            {
                var y = head - trail * 4;
                if (y < 0 || y >= height) continue;
                var fade = 1.0 - trail / 18.0;
                var g = ProcColor.Clamp(40 + fade * 200);
                var x0 = c * colW;
                for (var x = x0; x < Math.Min(width, x0 + colW - 2); x++)
                {
                    ProcColor.Set(bgra, y * width + x, 0, g, 0);
                }
            }
        }
    }
}

/// <summary>Purple/pink horizon grid (mac Synthwave).</summary>
public sealed class SynthwaveEngine : IProceduralEngine
{
    public string Id => "synthwave";
    public string DisplayName => "Synthwave";
    public string MacSource => "Engines/Procedural/SynthwaveView.swift";
    public void Reset(int seed = 1) { }

    public void RenderFrame(Span<byte> bgra, int width, int height, double t)
    {
        var horizon = (int)(height * 0.42);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                byte r, g, b;
                if (y < horizon)
                {
                    var k = y / (double)horizon;
                    r = ProcColor.Clamp(20 + k * 40);
                    g = ProcColor.Clamp(5 + k * 10);
                    b = ProcColor.Clamp(40 + k * 90);
                    // sun
                    var cx = width * 0.5;
                    var cy = horizon * 0.75;
                    var d = Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d < width * 0.12)
                    {
                        r = 255;
                        g = ProcColor.Clamp(120 + d);
                        b = 40;
                    }
                }
                else
                {
                    var ny = (y - horizon) / (double)(height - horizon + 1);
                    r = ProcColor.Clamp(30 + ny * 80);
                    g = 0;
                    b = ProcColor.Clamp(50 + ny * 100);
                    // perspective grid
                    var scroll = t * 40;
                    var gy = (int)((ny * 40 + scroll) % 8);
                    var persp = 0.2 + ny * 2.5;
                    var gx = (int)(((x - width / 2.0) / (8 * persp) + width) % 8);
                    if (gy == 0 || gx == 0)
                    {
                        r = 255;
                        g = 80;
                        b = 200;
                    }
                }

                ProcColor.Set(bgra, y * width + x, b, g, r);
            }
        }
    }
}

/// <summary>Flying stars (mac Starfield).</summary>
public sealed class StarfieldEngine : IProceduralEngine
{
    private readonly List<(float x, float y, float z)> _stars = new();

    public string Id => "starfield";
    public string DisplayName => "Starfield";
    public string MacSource => "Engines/Procedural/StarfieldView.swift";

    public void Reset(int seed = 1)
    {
        _stars.Clear();
        var rng = new Random(seed);
        for (var i = 0; i < 400; i++)
        {
            _stars.Add((
                (float)(rng.NextDouble() * 2 - 1),
                (float)(rng.NextDouble() * 2 - 1),
                (float)(0.1 + rng.NextDouble() * 0.9)));
        }
    }

    public void RenderFrame(Span<byte> bgra, int width, int height, double t)
    {
        if (_stars.Count == 0)
        {
            Reset();
        }

        ProcColor.Fill(bgra, 4, 4, 8);
        var cx = width / 2.0;
        var cy = height / 2.0;
        for (var i = 0; i < _stars.Count; i++)
        {
            var (x, y, z) = _stars[i];
            z -= (float)(0.012 + (i % 5) * 0.001);
            if (z <= 0.05f)
            {
                z = 1f;
            }

            _stars[i] = (x, y, z);
            var sx = (int)(cx + x / z * cx);
            var sy = (int)(cy + y / z * cy);
            if (sx < 0 || sy < 0 || sx >= width || sy >= height)
            {
                continue;
            }

            var bright = ProcColor.Clamp(80 + (1 - z) * 175);
            ProcColor.Set(bgra, sy * width + sx, bright, bright, bright);
            if (z < 0.4 && sx + 1 < width)
            {
                ProcColor.Set(bgra, sy * width + sx + 1, bright, bright, bright);
            }
        }
    }
}

/// <summary>Floating nodes + links (mac Neural Network).</summary>
public sealed class NeuralNetworkEngine : IProceduralEngine
{
    private readonly List<(float x, float y, float vx, float vy)> _nodes = new();

    public string Id => "neural-network";
    public string DisplayName => "Neural Network";
    public string MacSource => "Engines/Procedural/NeuralNetworkView.swift";

    public void Reset(int seed = 1)
    {
        _nodes.Clear();
        var rng = new Random(seed);
        for (var i = 0; i < 36; i++)
        {
            _nodes.Add((
                (float)rng.NextDouble(),
                (float)rng.NextDouble(),
                (float)(rng.NextDouble() * 0.04 - 0.02),
                (float)(rng.NextDouble() * 0.04 - 0.02)));
        }
    }

    public void RenderFrame(Span<byte> bgra, int width, int height, double t)
    {
        if (_nodes.Count == 0)
        {
            Reset();
        }

        ProcColor.Fill(bgra, 12, 10, 18);
        // integrate
        for (var i = 0; i < _nodes.Count; i++)
        {
            var (x, y, vx, vy) = _nodes[i];
            x += vx * 0.016f;
            y += vy * 0.016f;
            if (x < 0 || x > 1) vx = -vx;
            if (y < 0 || y > 1) vy = -vy;
            x = Math.Clamp(x, 0, 1);
            y = Math.Clamp(y, 0, 1);
            _nodes[i] = (x, y, vx, vy);
        }

        // edges
        for (var i = 0; i < _nodes.Count; i++)
        {
            for (var j = i + 1; j < _nodes.Count; j++)
            {
                var dx = _nodes[i].x - _nodes[j].x;
                var dy = _nodes[i].y - _nodes[j].y;
                var d2 = dx * dx + dy * dy;
                if (d2 > 0.04)
                {
                    continue;
                }

                DrawLine(bgra, width, height,
                    (int)(_nodes[i].x * (width - 1)), (int)(_nodes[i].y * (height - 1)),
                    (int)(_nodes[j].x * (width - 1)), (int)(_nodes[j].y * (height - 1)),
                    180, 80, 220);
            }
        }

        // nodes
        foreach (var n in _nodes)
        {
            var px = (int)(n.x * (width - 1));
            var py = (int)(n.y * (height - 1));
            for (var oy = -2; oy <= 2; oy++)
            for (var ox = -2; ox <= 2; ox++)
            {
                var x = px + ox;
                var y = py + oy;
                if (x < 0 || y < 0 || x >= width || y >= height) continue;
                ProcColor.Set(bgra, y * width + x, 220, 140, 255);
            }
        }
    }

    private static void DrawLine(Span<byte> bgra, int w, int h, int x0, int y0, int x1, int y1, byte b, byte g, byte r)
    {
        var dx = Math.Abs(x1 - x0);
        var dy = Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;
        while (true)
        {
            if ((uint)x0 < (uint)w && (uint)y0 < (uint)h)
            {
                ProcColor.Set(bgra, y0 * w + x0, b, g, r, 180);
            }

            if (x0 == x1 && y0 == y1) break;
            var e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
}

/// <summary>Slow multi-stop gradient drift (mac gradient family).</summary>
public sealed class AnimatedGradientEngine : IProceduralEngine
{
    public string Id => "animated-gradient";
    public string DisplayName => "Animated Gradient";
    public string MacSource => "Engines/Procedural (gradient family)";
    public void Reset(int seed = 1) { }

    public void RenderFrame(Span<byte> bgra, int width, int height, double t)
    {
        var a = t * 0.15;
        for (var y = 0; y < height; y++)
        {
            var v = y / (double)height;
            for (var x = 0; x < width; x++)
            {
                var u = x / (double)width;
                var r = ProcColor.Clamp((0.5 + 0.5 * Math.Sin(u * 3 + a)) * 200 + v * 40);
                var g = ProcColor.Clamp((0.5 + 0.5 * Math.Sin(v * 4 - a * 1.3)) * 120 + 20);
                var b = ProcColor.Clamp((0.5 + 0.5 * Math.Sin((u + v) * 2 + a * 0.7)) * 220);
                ProcColor.Set(bgra, y * width + x, b, g, r);
            }
        }
    }
}
