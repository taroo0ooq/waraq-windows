// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// WRQ-WIN-002 Phase 6-QA — Gallery privacy + URL policy matrix (Stagecraft QA).

using System.Net;
using System.Net.Http;
using System.Text;
using Waraq.Windows.Core;
using Waraq.Windows.Core.Gallery;
using Waraq.Windows.Engines.Procedural;
using Waraq.Windows.Host;

namespace Waraq.Windows.Tests;

public class GalleryPrivacyTests
{
    [Fact]
    public void ApiKeyStore_RoundTrip_AndBlankClears()
    {
        var path = Path.Combine(Path.GetTempPath(), "waraq-keys-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var store = new ApiKeyStore(path);
            Assert.False(store.HasKey(GallerySourceKind.Pixabay));
            store.SetKey(GallerySourceKind.Pixabay, "  abc123  ");
            Assert.True(store.HasKey(GallerySourceKind.Pixabay));
            Assert.Equal("abc123", store.GetKey(GallerySourceKind.Pixabay));

            var store2 = new ApiKeyStore(path);
            Assert.Equal("abc123", store2.GetKey(GallerySourceKind.Pixabay));

            store2.SetKey(GallerySourceKind.Pixabay, "   ");
            Assert.False(store2.HasKey(GallerySourceKind.Pixabay));
        }
        finally
        {
            try { File.Delete(path); } catch { /* ok */ }
        }
    }

    [Fact]
    public void Nasa_DoesNotRequireApiKey()
    {
        var nasa = GallerySourceInfo.Get(GallerySourceKind.Nasa);
        Assert.False(nasa.RequiresApiKey);
        Assert.True(GallerySourceInfo.Get(GallerySourceKind.Pixabay).RequiresApiKey);
        Assert.True(GallerySourceInfo.Get(GallerySourceKind.Pexels).RequiresApiKey);
    }

    [Fact]
    public void BrowseWeb_RejectsNonHttp()
    {
        Assert.Throws<ArgumentException>(() => BrowseWeb.Open("file:///c:/temp"));
        Assert.Throws<ArgumentException>(() => BrowseWeb.Open("not-a-url"));
        Assert.Throws<ArgumentException>(() => BrowseWeb.Open("ftp://example.com"));
    }

    [Fact]
    public void ExternalBrowse_FourSites_HttpsOnly()
    {
        Assert.Equal(4, ExternalBrowseCatalog.All.Count);
        Assert.All(ExternalBrowseCatalog.All, s =>
        {
            Assert.StartsWith("https://", s.WebsiteUrl, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task Search_UsesCache_SecondCallNoNetwork()
    {
        var root = Path.Combine(Path.GetTempPath(), "waraq-gcache-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var keysPath = Path.Combine(root, "keys.json");
        var cacheDir = Path.Combine(root, "cache");
        try
        {
            var keys = new ApiKeyStore(keysPath);
            keys.SetKey(GallerySourceKind.Pixabay, "test-key");
            var cache = new GalleryCache(cacheDir);

            var handler = new StubHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """{"hits":[{"id":1,"pageURL":"https://pixabay.com/x","user":"a","tags":"ocean","videos":{"medium":{"url":"https://cdn.example/a.mp4"}}}]}""",
                        Encoding.UTF8,
                        "application/json"),
                });
            var http = new HttpClient(handler);
            var svc = new GallerySearchService(keys, cache, http);

            var a = await svc.SearchAsync(GallerySourceKind.Pixabay, "ocean");
            Assert.Single(a);
            Assert.Equal(1, svc.NetworkCallCount);

            var b = await svc.SearchAsync(GallerySourceKind.Pixabay, "ocean");
            Assert.Single(b);
            Assert.Equal(1, svc.NetworkCallCount); // cache hit — no second network
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { /* ok */ }
        }
    }

    [Fact]
    public async Task Search_MissingPixabayKey_Throws_WithoutNetwork()
    {
        var root = Path.Combine(Path.GetTempPath(), "waraq-g2-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var keys = new ApiKeyStore(Path.Combine(root, "k.json"));
            var cache = new GalleryCache(Path.Combine(root, "c"));
            var handler = new StubHandler(_ => throw new InvalidOperationException("network should not run"));
            var svc = new GallerySearchService(keys, cache, new HttpClient(handler));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.SearchAsync(GallerySourceKind.Pixabay, "x"));
            Assert.Equal(0, svc.NetworkCallCount);
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { /* ok */ }
        }
    }

    [Fact]
    public void NasaId_PathEncoding_EscapesSpaces()
    {
        var nasaId = "Seeing Earth as Only NASA Can";
        var pathId = string.Join("/", nasaId.Split('/').Select(Uri.EscapeDataString));
        Assert.Equal("Seeing%20Earth%20as%20Only%20NASA%20Can", pathId);
    }

    [Theory]
    [InlineData("http://example.com/a.mp4")]
    [InlineData("file:///c:/temp/a.mp4")]
    [InlineData("ftp://example.com/a.mp4")]
    [InlineData("https://127.0.0.1/a.mp4")]
    [InlineData("https://192.168.1.10/a.mp4")]
    [InlineData("https://10.0.0.5/a.mp4")]
    [InlineData("https://172.16.0.1/a.mp4")]
    [InlineData("https://169.254.1.1/a.mp4")]
    [InlineData("https://localhost/a.mp4")]
    [InlineData("https://foo.localhost/a.mp4")]
    [InlineData("https://user:pass@cdn.example.com/a.mp4")]
    [InlineData("")]
    [InlineData("not-a-url")]
    public void GalleryUrlPolicy_RejectsUnsafe(string url)
    {
        Assert.Throws<InvalidOperationException>(() =>
            GalleryUrlPolicy.EnsureSafeHttpsUrl(url, "test"));
    }

    [Fact]
    public void GalleryUrlPolicy_AllowsPublicHttps()
    {
        GalleryUrlPolicy.EnsureSafeHttpsUrl("https://cdn.example.com/video.mp4", "test");
        GalleryUrlPolicy.EnsureSafeHttpsUrl("https://images-assets.nasa.gov/video/x/x~orig.mp4", "test");
    }

    [Fact]
    public void GalleryUrlPolicy_MaxDownloadConstant_Is512MiB()
    {
        Assert.Equal(512L * 1024 * 1024, GalleryUrlPolicy.MaxDownloadBytes);
    }

    [Fact]
    public void HostProbe_NoRegression_WithGalleryTypesLoaded()
    {
        _ = ExternalBrowseCatalog.All.Count;
        _ = GalleryUrlPolicy.MaxDownloadBytes;
        var probe = new DesktopWallpaperHost().Probe();
        Assert.False(string.IsNullOrWhiteSpace(probe.Message));
    }

    [Fact]
    public void ProceduralCatalog_NoRegression_StillSix()
    {
        Assert.Equal(6, ProceduralCatalog.All.Count);
        Assert.Contains(ProceduralCatalog.All, d => d.Id == "aurora");
    }

    [Fact]
    public void LibraryImport_NoRegression_WithGalleryPresent()
    {
        var root = Path.Combine(Path.GetTempPath(), "waraq-g3-" + Guid.NewGuid().ToString("N"));
        var paths = new LibraryPaths(root);
        paths.EnsureDirectories();
        try
        {
            var gif = Path.Combine(root, "s.gif");
            File.WriteAllBytes(gif, Convert.FromBase64String(
                "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"));
            var store = new WallpaperLibraryStore(paths);
            var entry = store.Import(gif);
            Assert.Equal("Gif", entry.Kind);
            Assert.True(File.Exists(store.ResolveAbsolute(entry.RelativePath)));
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { /* ok */ }
        }
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _impl;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> impl) => _impl = impl;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_impl(request));
    }
}
