// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Phase 6-Secure: validate gallery download URLs (no SSRF / private hosts).

using System.Net;
using System.Net.Sockets;

namespace Waraq.Windows.Core.Gallery;

/// <summary>
/// Safety policy for gallery-originated HTTPS URLs (search results → import download).
/// API JSON is untrusted: never fetch arbitrary schemes/hosts into the library.
/// </summary>
public static class GalleryUrlPolicy
{
    /// <summary>Soft cap for user-initiated gallery import downloads.</summary>
    public const long MaxDownloadBytes = 512L * 1024 * 1024; // 512 MiB

    public static void EnsureSafeHttpsUrl(string? url, string purpose)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException($"{purpose}: URL is empty.");
        }

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"{purpose}: URL is not absolute.");
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{purpose}: only https:// URLs are allowed.");
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new InvalidOperationException($"{purpose}: URLs with userinfo are not allowed.");
        }

        var host = uri.IdnHost;
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException($"{purpose}: host is missing.");
        }

        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{purpose}: local hostnames are not allowed.");
        }

        // Block literal private/reserved IPs in the URL host.
        if (IPAddress.TryParse(host, out var literalIp) && IsBlockedAddress(literalIp))
        {
            throw new InvalidOperationException($"{purpose}: private/reserved IP hosts are not allowed.");
        }

        // Best-effort DNS check (blocks obvious SSRF to LAN). Race possible; still raises bar.
        try
        {
            var addresses = Dns.GetHostAddresses(host);
            foreach (var addr in addresses)
            {
                if (IsBlockedAddress(addr))
                {
                    throw new InvalidOperationException(
                        $"{purpose}: host resolves to a private/reserved address.");
                }
            }
        }
        catch (SocketException)
        {
            // Let HTTP client surface resolution failure later.
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException($"{purpose}: host could not be resolved safely.");
        }
    }

    public static async Task DownloadToFileAsync(
        HttpClient http,
        string url,
        string destinationPath,
        CancellationToken ct = default)
    {
        EnsureSafeHttpsUrl(url, "Gallery download");

        using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
            .ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gallery download HTTP {(int)resp.StatusCode}");
        }

        if (resp.Content.Headers.ContentLength is long declared && declared > MaxDownloadBytes)
        {
            throw new InvalidOperationException(
                $"Gallery download too large (Content-Length {declared} > {MaxDownloadBytes}).");
        }

        await using var input = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        await using var output = new FileStream(
            destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

        var buffer = new byte[81920];
        long total = 0;
        int read;
        while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
        {
            total += read;
            if (total > MaxDownloadBytes)
            {
                throw new InvalidOperationException(
                    $"Gallery download exceeded {MaxDownloadBytes} bytes.");
            }

            await output.WriteAsync(buffer.AsMemory(0, read), ct).ConfigureAwait(false);
        }

        if (total <= 0)
        {
            throw new InvalidOperationException("Gallery download was empty.");
        }
    }

    private static bool IsBlockedAddress(IPAddress addr)
    {
        if (IPAddress.IsLoopback(addr))
        {
            return true;
        }

        if (addr.IsIPv6LinkLocal || addr.IsIPv6SiteLocal || addr.IsIPv6Multicast)
        {
            return true;
        }

        if (addr.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = addr.GetAddressBytes();
            // 10.0.0.0/8
            if (b[0] == 10) return true;
            // 172.16.0.0/12
            if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return true;
            // 192.168.0.0/16
            if (b[0] == 192 && b[1] == 168) return true;
            // 127.0.0.0/8 already loopback
            // 169.254.0.0/16 link-local
            if (b[0] == 169 && b[1] == 254) return true;
            // 0.0.0.0/8
            if (b[0] == 0) return true;
            // 100.64.0.0/10 CGNAT
            if (b[0] == 100 && b[1] >= 64 && b[1] <= 127) return true;
        }

        return false;
    }
}
