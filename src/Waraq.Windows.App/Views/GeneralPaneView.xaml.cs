// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Waraq.Windows.Core;

namespace Waraq.Windows.App.Views;

public sealed partial class GeneralPaneView : UserControl
{
    public GeneralPaneView()
    {
        InitializeComponent();
        StatusLine.Text =
            $"{AppInfo.StatusLine}\nOnboarding completed: {AppServices.OnboardingState.HasCompleted}";
    }

    private void OnRerunOnboarding(object sender, RoutedEventArgs e)
    {
        App.ShowOnboardingAgain();
        StatusLine.Text =
            $"{AppInfo.StatusLine}\nOnboarding completed: {AppServices.OnboardingState.HasCompleted} (wizard opened)";
    }
}
