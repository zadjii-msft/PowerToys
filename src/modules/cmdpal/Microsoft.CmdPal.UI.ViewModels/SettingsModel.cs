// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerToys.Settings.UI.Library;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class SettingsModel(IServiceProvider _serviceProvider) : ObservableObject
{
    public string TestString { get; set; } = string.Empty;

    public HotkeySettings? Hotkey { get; set; } = new HotkeySettings(true, true, false, false, 0xBE);

    public Dictionary<string, ProviderSettings> ProviderSettings { get; set; } = [];

    public void FooBar()
    {
        _ = _serviceProvider.GetService<TopLevelCommandManager>();
    }
}
