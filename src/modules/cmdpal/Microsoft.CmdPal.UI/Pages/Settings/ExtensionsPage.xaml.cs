// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.UI.Pages;

public sealed partial class ExtensionsPage : Page
{
    private readonly TaskScheduler _mainTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

    public SettingsViewModel? ViewModel;

    public ExtensionsPage()
    {
        this.InitializeComponent();

        var settings = App.Current.Services.GetService<SettingsModel>()!;
        ViewModel = new SettingsViewModel(settings, App.Current.Services, _mainTaskScheduler);
    }
}
