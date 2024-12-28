// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsModel _settings;

    [ObservableProperty]
    public partial bool IsCardEnabled { get; set; } = true;

    public ObservableCollection<string> CommandProviders { get; } = [];

    public SettingsViewModel(SettingsModel settings)
    {
        _settings = settings;

        foreach (var item in _settings.GetCommandProviders())
        {
            CommandProviders.Add(item);
        }
    }
}
