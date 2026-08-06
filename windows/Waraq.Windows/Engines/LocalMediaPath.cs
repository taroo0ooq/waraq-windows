// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.IO;

namespace Waraq.Windows.Engines;

/// <summary>
/// Validates that wallpaper media is a real local filesystem file (not UNC/URL/device).
/// Phase 3 Secure hardening (WRQ-WIN-001): keep zero unintended network egress via media load.
/// </summary>
public static class LocalMediaPath
{
    /// <summary>Soft cap for GIF decode-into-memory (GifBitmapDecoder OnLoad).</summary>
    public const long MaxGifBytes = 64L * 1024 * 1024;

    /// <summary>Soft cap for video open (MF still streams; blocks absurd inputs).</summary>
    public const long MaxVideoBytes = 4L * 1024 * 1024 * 1024;

    /// <summary>
    /// Normalize to a full local path and enforce allow rules.
    /// Throws <see cref="FileNotFoundException"/>, <see cref="NotSupportedException"/>, or <see cref="IOException"/>.
    /// </summary>
    public static string NormalizeExistingLocalFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Media path is required.", nameof(path));
        }

        var trimmed = path.Trim().Trim('"');

        // Reject explicit non-file URI schemes (http/https/smb/etc.).
        if (LooksLikeNonFileUri(trimmed))
        {
            throw new NotSupportedException(
                "Only local filesystem media is supported. Network URLs are not allowed.");
        }

        // file:///C:/... → local path
        if (trimmed.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var fileUri) ||
                !fileUri.IsFile ||
                fileUri.IsUnc)
            {
                throw new NotSupportedException(
                    "Only local file: URIs are supported (no UNC/network file URIs).");
            }

            trimmed = fileUri.LocalPath;
        }

        string full;
        try
        {
            full = Path.GetFullPath(trimmed);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new NotSupportedException("Media path could not be resolved to a local file.", ex);
        }

        if (IsUncOrDevicePath(full) || IsUncOrDevicePath(trimmed))
        {
            throw new NotSupportedException(
                "Network (UNC) and device paths are not allowed. Copy the media to a local drive first.");
        }

        // Require a normal drive-letter path (C:\...).
        if (full.Length < 3 ||
            !char.IsLetter(full[0]) ||
            full[1] != ':' ||
            (full[2] != Path.DirectorySeparatorChar && full[2] != Path.AltDirectorySeparatorChar))
        {
            throw new NotSupportedException(
                "Media must be on a local drive path (for example C:\\Videos\\loop.mp4).");
        }

        if (!File.Exists(full))
        {
            throw new FileNotFoundException("Media file not found.", full);
        }

        // Reject directories / reparse weirdness masquerading as files when possible.
        var attrs = File.GetAttributes(full);
        if ((attrs & FileAttributes.Directory) != 0)
        {
            throw new NotSupportedException("Media path must be a file, not a directory.");
        }

        return full;
    }

    /// <summary>Enforce per-kind soft size limits before decode/open.</summary>
    public static void EnsureWithinSizeLimit(string fullPath, MediaKind kind)
    {
        var length = new FileInfo(fullPath).Length;
        var limit = kind switch
        {
            MediaKind.Gif => MaxGifBytes,
            MediaKind.Video => MaxVideoBytes,
            _ => MaxGifBytes,
        };

        if (length <= 0)
        {
            throw new InvalidDataException("Media file is empty.");
        }

        if (length > limit)
        {
            var limitLabel = kind == MediaKind.Gif ? "64 MiB" : "4 GiB";
            throw new NotSupportedException(
                $"Media file is too large ({length} bytes). MVP limit for {kind} is {limitLabel}.");
        }
    }

    /// <summary>Build a file:// URI suitable for WPF media APIs from a validated local path.</summary>
    public static Uri ToFileUri(string fullLocalPath)
    {
        return new Uri(fullLocalPath, UriKind.Absolute);
    }

    private static bool LooksLikeNonFileUri(string value)
    {
        var schemeIdx = value.IndexOf("://", StringComparison.Ordinal);
        if (schemeIdx <= 0)
        {
            return false;
        }

        var scheme = value[..schemeIdx];
        return !scheme.Equals("file", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUncOrDevicePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        // \\server\share or //server/share
        if (path.StartsWith(@"\\", StringComparison.Ordinal) ||
            path.StartsWith("//", StringComparison.Ordinal))
        {
            return true;
        }

        // \\?\C:\... is local extended; \\?\UNC\ is network.
        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(@"//?/UNC/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Extended local \\?\C:\ is OK if it has a drive letter after the prefix —
        // but GetFullPath usually strips to C:\. Treat bare \\?\ without drive as device.
        if (path.StartsWith(@"\\.\", StringComparison.Ordinal) ||
            path.StartsWith("//./", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
