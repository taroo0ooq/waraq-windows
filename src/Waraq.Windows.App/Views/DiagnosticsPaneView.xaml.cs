// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Waraq.Windows.Core;
using Waraq.Windows.Host;

namespace Waraq.Windows.App.Views;

public sealed partial class DiagnosticsPaneView : UserControl
{
    private readonly ResourceMonitor _monitor = new();

    public DiagnosticsPaneView()
    {
        InitializeComponent();
        Refresh();
    }

    private void OnRefresh(object sender, RoutedEventArgs e) => Refresh();

    private void Refresh()
    {
        var r = _monitor.Sample();
        // second sample improves CPU delta
        System.Threading.Thread.Sleep(80);
        r = _monitor.Sample();
        var b = SystemPowerProbe.Sample();
        var probe = App.Wallpaper.Probe();
        var g = AppServices.GovernorRuntime;

        SampleText.Text =
            $"App: {AppInfo.ProductName} {AppInfo.Version}\n" +
            $"Host strategy: {App.Wallpaper.StrategyName} · workerW={probe.FoundWorkerW} · {probe.Message}\n" +
            $"Wallpaper: running={App.Wallpaper.IsRunning} paused={App.Wallpaper.IsPlaybackPaused}\n" +
            $"  kind={App.Wallpaper.ActiveKind} path={App.Wallpaper.ActivePath ?? App.Wallpaper.ActiveProceduralId ?? "—"}\n" +
            $"Process CPU ~{r.ProcessCpuPercent:0.0}% (best-effort)\n" +
            $"Working set: {r.WorkingSetBytes / (1024 * 1024)} MB\n" +
            $"Private bytes: {r.PrivateBytes / (1024 * 1024)} MB\n" +
            $"Threads: {r.ThreadCount}\n" +
            $"Battery: has={b.HasBattery} onBattery={b.IsOnBattery} pct={b.Percent}\n" +
            $"Governor: {(g is null ? "n/a" : g.LastDecision.Detail)}\n" +
            $"Displays: {AppServices.RefreshDisplays().Count}\n" +
            $"Library items: {AppServices.LibraryStore.Items.Count}\n" +
            "Telemetry: none";
    }
}
