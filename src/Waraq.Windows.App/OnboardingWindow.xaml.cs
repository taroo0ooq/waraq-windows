// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Waraq.Windows.Core;

namespace Waraq.Windows.App;

public sealed partial class OnboardingWindow : Window
{
    private int _step;
    private readonly string[] _bodies =
    [
        "Animated wallpapers for Windows — GPL-3.0, privacy-first, local by default.",
        "Gallery contacts the network only when you press Search. Browse Web opens your browser; we never scrape external sites. Zero telemetry.",
        "Import video/GIF into your local Library under %AppData%\\Waraq. Apply from Library or Wallpapers.",
        "Performance governor can pause playback on low battery, fullscreen games, or high memory — all decided on-device.",
        "Use the tray icon for Settings, pause, and quit. You can re-run this wizard from General anytime.",
    ];

    public OnboardingWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new global::Windows.Graphics.SizeInt32(560, 360));
        ShowStep();
    }

    private void ShowStep()
    {
        var steps = OnboardingStateStore.Steps;
        _step = Math.Clamp(_step, 0, steps.Count - 1);
        StepTitle.Text = steps[_step];
        StepBody.Text = _bodies[_step];
        StepIndicator.Text = $"Step {_step + 1} of {steps.Count}";
        BackButton.IsEnabled = _step > 0;
        NextButton.Content = _step >= steps.Count - 1 ? "Finish" : "Next";
    }

    private void OnBack(object sender, RoutedEventArgs e)
    {
        _step--;
        ShowStep();
    }

    private void OnNext(object sender, RoutedEventArgs e)
    {
        if (_step >= OnboardingStateStore.Steps.Count - 1)
        {
            AppServices.OnboardingState.MarkCompleted();
            Close();
            return;
        }

        _step++;
        ShowStep();
    }
}
