// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Waraq.Windows.Core;
using Waraq.Windows.Host;

namespace Waraq.Windows.App;

/// <summary>Process-wide library + profile stores (Phase 4).</summary>
public static class AppServices
{
    private static readonly Lazy<LibraryPaths> Paths = new(() => new LibraryPaths());
    private static readonly Lazy<WallpaperLibraryStore> Library =
        new(() => new WallpaperLibraryStore(Paths.Value));
    private static readonly Lazy<DisplayProfileStore> Profiles =
        new(() => new DisplayProfileStore(Paths.Value));

    public static LibraryPaths LibraryPaths => Paths.Value;
    public static WallpaperLibraryStore LibraryStore => Library.Value;
    public static DisplayProfileStore ProfileStore => Profiles.Value;

    public static IReadOnlyList<DisplayInfo> RefreshDisplays() =>
        DisplayEnumerator.EnumerateActiveDisplays();
}
