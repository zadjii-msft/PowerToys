// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CmdPal.Common.Services;
using Microsoft.CmdPal.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class TopLevelCommandManager(IServiceProvider _serviceProvider)
{
    private IEnumerable<ICommandProvider>? _builtInCommands;

    // public ObservableCollection<CommandProviderWrapper> ActionsProvider { get; set; } = [];
    public ObservableCollection<TopLevelCommandWrapper> TopLevelCommands { get; set; } = [];

    // [RelayCommand]
    public async Task<bool> LoadBuiltinsAsync()
    {
        _builtInCommands = _serviceProvider.GetServices<ICommandProvider>();

        // Load Built In Commands First
        foreach (var provider in _builtInCommands)
        {
            CommandProviderWrapper wrapper = new(provider);

            // ActionsProvider.Add(wrapper);
            await LoadTopLevelCommandsFromProvider(wrapper);
        }

        return true;
    }

    private async Task LoadTopLevelCommandsFromProvider(CommandProviderWrapper commandProvider)
    {
        await commandProvider.LoadTopLevelCommands();
        foreach (var i in commandProvider.TopLevelItems)
        {
            TopLevelCommands.Add(new(new(i)));
        }
    }

    [RelayCommand]
    public async Task<bool> LoadExtensionsAsync()
    {
        var extensionService = _serviceProvider.GetService<IExtensionService>()!;
        var extensions = await extensionService.GetInstalledExtensionsAsync();
        foreach (var extension in extensions)
        {
            try
            {
                await extension.StartExtensionAsync();
                CommandProviderWrapper wrapper = new(extension);
                await LoadTopLevelCommandsFromProvider(wrapper);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            // ActionsProvider.Add(wrapper);
        }

        return true;
    }
}
