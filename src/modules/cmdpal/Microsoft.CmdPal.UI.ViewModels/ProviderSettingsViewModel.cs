// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class ProviderSettingsViewModel(CommandProviderWrapper _provider, ProviderSettings _providerSettings, TopLevelCommandManager _tlcManager) : ObservableObject
{
    public string DisplayName => _provider.DisplayName;

    public string ExtensionName => _provider.Extension?.ExtensionDisplayName ?? "Built-in";

    public string ExtensionSubtext => $"{ExtensionName}, {TopLevelCommands.Count} commands";

    public IconInfoViewModel Icon => _provider.Icon;

    public bool IsEnabled
    {
        get => _providerSettings.IsEnabled;
        set => _providerSettings.IsEnabled = value;
    }

    public List<TopLevelCommandItemWrapper> TopLevelCommands
    {
        get
        {
            var topLevelCommands = _tlcManager.TopLevelCommands;
            var thisProvider = _provider;
            var providersCommands = thisProvider.TopLevelItems;
            List<TopLevelCommandItemWrapper> results = [];
            foreach (var command in providersCommands)
            {
                var match = topLevelCommands.Where(tlc => tlc.Model.Unsafe == command).FirstOrDefault();
                if (match != null)
                {
                    results.Add(match);
                }
            }

            return results;
        }
    }
}
