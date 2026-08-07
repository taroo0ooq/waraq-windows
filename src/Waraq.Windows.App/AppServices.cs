// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Waraq.Windows.Core;
using Waraq.Windows.Core.Gallery;
using Waraq.Windows.Host;

namespace Waraq.Windows.App;

/// <summary>Process-wide services (library, profiles, gallery, governor).</summary>
public static class AppServices
{
    private static readonly Lazy<LibraryPaths> Paths = new(() => new LibraryPaths());
    private static readonly Lazy<WallpaperLibraryStore> Library =
        new(() => new WallpaperLibraryStore(Paths.Value));
    private static readonly Lazy<DisplayProfileStore> Profiles =
        new(() => new DisplayProfileStore(Paths.Value));
    private static readonly Lazy<ApiKeyStore> Keys = new(() => new ApiKeyStore());
    private static readonly Lazy<GalleryCache> Cache = new(() => new GalleryCache());
    private static readonly Lazy<GallerySearchService> Gallery =
        new(() => new GallerySearchService(Keys.Value, Cache.Value));
    private static readonly Lazy<GovernorSettingsStore> Governor =
        new(() => new GovernorSettingsStore());
    private static readonly Lazy<OnboardingStateStore> Onboarding =
        new(() => new OnboardingStateStore());

    public static LibraryPaths LibraryPaths => Paths.Value;
    public static WallpaperLibraryStore LibraryStore => Library.Value;
    public static DisplayProfileStore ProfileStore => Profiles.Value;
    public static ApiKeyStore ApiKeys => Keys.Value;
    public static GallerySearchService GallerySearch => Gallery.Value;
    public static GovernorSettingsStore GovernorSettings => Governor.Value;
    public static OnboardingStateStore OnboardingState => Onboarding.Value;

    public static GovernorRuntime? GovernorRuntime { get; set; }

    public static IReadOnlyList<DisplayInfo> RefreshDisplays() =>
        DisplayEnumerator.EnumerateActiveDisplays();
}
