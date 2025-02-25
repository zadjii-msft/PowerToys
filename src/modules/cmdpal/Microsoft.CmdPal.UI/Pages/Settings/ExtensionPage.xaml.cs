// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace Microsoft.CmdPal.UI.Pages;

public sealed partial class ExtensionPage : Page
{
    private readonly TaskScheduler _mainTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

    public ProviderSettingsViewModel? ViewModel { get; private set; }

    public ExtensionPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = e.Parameter is ProviderSettingsViewModel vm
            ? vm
            : throw new ArgumentException($"{nameof(ExtensionPage)} navigation args should be passed a {nameof(ProviderSettingsViewModel)}");
    }

    private void AliasTextBox_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            if (sender is TextBox textBox)
            {
                // textBox.Focus(Microsoft.UI.Xaml.FocusState.Keyboard);
                textBox.Text = textBox.Text;
            }
        }
    }
}
