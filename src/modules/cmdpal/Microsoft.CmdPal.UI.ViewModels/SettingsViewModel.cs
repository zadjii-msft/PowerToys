// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerToys.Settings.UI.Library;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsModel _settings;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    public partial bool IsCardEnabled { get; set; } = true;

    public HotkeySettings? Hotkey
    {
        get => _settings.Hotkey;
        set
        {
            _settings.Hotkey = value;
            _settings.Save();
        }
    }

    public ObservableCollection<ProviderSettingsViewModel> CommandProviders { get; } = [];

    public SettingsViewModel(SettingsModel settings, IServiceProvider serviceProvider)
    {
        _settings = settings;
        _serviceProvider = serviceProvider;

        var activeProviders = GetCommandProviders();
        var allProviderSettings = _settings.ProviderSettings;

        foreach (var item in activeProviders)
        {
            if (!allProviderSettings.ContainsKey(item.ProviderId))
            {
                allProviderSettings[item.ProviderId] = new ProviderSettings(item);
            }

            var providerSettings = allProviderSettings.TryGetValue(item.ProviderId, out var value) ?
                value :
                new ProviderSettings(item);

            var settingsModel = new ProviderSettingsViewModel(item, providerSettings);
            CommandProviders.Add(settingsModel);
        }
    }

    private IEnumerable<CommandProviderWrapper> GetCommandProviders()
    {
        var manager = _serviceProvider.GetService<TopLevelCommandManager>()!;
        var allProviders = manager.CommandProviders;
        return allProviders;
    }

    public void Save()
    {
        _settings.Save();
    }
}
