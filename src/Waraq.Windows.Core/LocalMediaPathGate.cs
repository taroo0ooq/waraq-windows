// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using System.IO;

namespace Waraq.Windows.Core;

/// <summary>
/// Local media path gate (WRQ-WIN-001 lessons + WRQ-WIN-002 Phase 3-Secure harden).
/// Rejects empty, non-rooted, UNC, URL schemes, and device namespaces.
/// NormalizeExisting re-validates after GetFullPath and enforces drive-letter local paths.
/// </summary>
public static class LocalMediaPathGate
{
    public const long MaxGifBytes = 64L * 1024 * 1024;
    public const long MaxVideoBytes = 4L * 1024 * 1024 * 1024;

    public static bool IsAllowed(string? path, out string reason)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            reason = "Path is empty.";
            return false;
        }

        var trimmed = path.Trim().Trim('"');
        return IsAllowedCore(trimmed, out reason);
    }

    /// <summary>
    /// Normalize to full local path, re-gate after resolution, ensure file exists.
    /// </summary>
    public static string NormalizeExistingLocalFile(string? path)
    {
        if (!IsAllowed(path, out var reason))
        {
            throw new InvalidOperationException($"Media path rejected: {reason}");
        }

        var trimmed = path!.Trim().Trim('"');
        string full;
        try
        {
            full = Path.GetFullPath(trimmed);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or IOException)
        {
            throw new InvalidOperationException("Media path could not be resolved to a local file.", ex);
        }

        if (!IsAllowedCore(full, out reason))
        {
            throw new InvalidOperationException($"Media path rejected after normalize: {reason}");
        }

        if (!IsDriveLetterPath(full))
        {
            throw new InvalidOperationException(
                "Media must be on a local drive path (for example C:\\Videos\\loop.mp4).");
        }

        if (!File.Exists(full))
        {
            throw new FileNotFoundException("Media file not found.", full);
        }

        var attrs = File.GetAttributes(full);
        if ((attrs & FileAttributes.Directory) != 0)
        {
            throw new InvalidOperationException("Media path must be a file, not a directory.");
        }

        return full;
    }

    public static void EnsureWithinSizeLimit(string fullPath, MediaKind kind)
    {
        var length = new FileInfo(fullPath).Length;
        if (length <= 0)
        {
            throw new InvalidDataException("Media file is empty.");
        }

        var limit = kind == MediaKind.Gif ? MaxGifBytes : MaxVideoBytes;
        if (length > limit)
        {
            var label = kind == MediaKind.Gif ? "64 MiB" : "4 GiB";
            throw new InvalidOperationException(
                $"Media file is too large ({length} bytes). Limit for {kind} is {label}.");
        }
    }

    public static void EnsureAllowed(string? path)
    {
        if (!IsAllowed(path, out var reason))
        {
            throw new InvalidOperationException($"Media path rejected: {reason}");
        }
    }

    private static bool IsAllowedCore(string trimmed, out string reason)
    {
        if (trimmed.Contains("://", StringComparison.Ordinal) ||
            trimmed.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("smb:", StringComparison.OrdinalIgnoreCase))
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

        // Device namespaces (usually already caught as UNC prefix)
        if (trimmed.StartsWith(@"\\.\", StringComparison.Ordinal) ||
            trimmed.StartsWith(@"\\?\", StringComparison.Ordinal) ||
            trimmed.StartsWith("//./", StringComparison.Ordinal))
        {
            reason = "Device namespace paths are not allowed.";
            return false;
        }

        reason = "OK";
        return true;
    }

    private static bool IsDriveLetterPath(string full)
    {
        return full.Length >= 3 &&
               char.IsLetter(full[0]) &&
               full[1] == ':' &&
               (full[2] == Path.DirectorySeparatorChar || full[2] == Path.AltDirectorySeparatorChar);
    }
}
