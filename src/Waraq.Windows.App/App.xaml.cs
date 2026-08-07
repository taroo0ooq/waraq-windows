// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Waraq.Windows.App.Tray;

namespace Waraq.Windows.App;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            // Keep tray teardown best-effort
            e.Handled = false;
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
