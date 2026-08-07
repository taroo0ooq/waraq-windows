// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

namespace Waraq.Windows.Core;

/// <summary>
/// Local media path gate (ported from WRQ-WIN-001 Phase 3 secure lessons).
/// Rejects empty, non-rooted, UNC, and URL schemes. Gallery downloads will
/// land as local paths after Phase 6.
/// </summary>
public static class LocalMediaPathGate
{
    public static bool IsAllowed(string? path, out string reason)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            reason = "Path is empty.";
            return false;
        }

        var trimmed = path.Trim();

        if (trimmed.Contains("://", StringComparison.Ordinal) ||
            trimmed.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            reason = "URL schemes are not allowed; use a local file path.";
            return false;
        }

        if (trimmed.StartsWith(@"\\", StringComparison.Ordinal) ||
            trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            reason = "UNC network paths are not allowed.";
            return false;
        }

        if (!Path.IsPathRooted(trimmed))
        {
            reason = "Path must be rooted (absolute).";
            return false;
        }

        // Reject device namespaces
        if (trimmed.StartsWith(@"\\.\", StringComparison.Ordinal) ||
            trimmed.StartsWith(@"\\?\", StringComparison.Ordinal))
        {
            reason = "Device namespace paths are not allowed.";
            return false;
        }

        reason = "OK";
        return true;
    }

    public static void EnsureAllowed(string? path)
    {
        if (!IsAllowed(path, out var reason))
        {
            throw new InvalidOperationException($"Media path rejected: {reason}");
        }
    }
}
