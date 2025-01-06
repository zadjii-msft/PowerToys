// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CmdPal.Common.Services;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class TopLevelCommandManager(IServiceProvider _serviceProvider) : ObservableObject
{
    private IEnumerable<ICommandProvider>? _builtInCommands;

    public ObservableCollection<TopLevelCommandWrapper> TopLevelCommands { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; private set; } = true;

    public async Task<bool> LoadBuiltinsAsync()
    {
        // Load built-In commands first. These are all in-proc, and
        // owned by our ServiceProvider.
        _builtInCommands = _serviceProvider.GetServices<ICommandProvider>();
        foreach (var provider in _builtInCommands)
        {
            CommandProviderWrapper wrapper = new(provider);
            await LoadTopLevelCommandsFromProvider(wrapper);
        }

        return true;
    }

    private async Task LoadTopLevelCommandsFromProvider(CommandProviderWrapper commandProvider)
    {
        await commandProvider.LoadTopLevelCommands();
        foreach (var i in commandProvider.TopLevelItems)
        {
            TopLevelCommands.Add(new(new(i), false));
        }

        foreach (var i in commandProvider.FallbackItems)
        {
            TopLevelCommands.Add(new(new(i), true));
        }

        commandProvider.CommandsChanged += CommandProvider_CommandsChanged;
    }

    private void CommandProvider_CommandsChanged(CommandProviderWrapper sender, ItemsChangedEventArgs args)
    {
        _ = Task.Run(async () => await UpdateCommandsForProvider(sender, args));
    }

    private async Task UpdateCommandsForProvider(CommandProviderWrapper sender, ItemsChangedEventArgs args)
    {
        List<TopLevelCommandWrapper> clone = [.. TopLevelCommands];
        List<TopLevelCommandWrapper> toRemove = [];

        foreach (var item in TopLevelCommands)
        {
            try
            {
                if (item.Command != null)
                {
                    foreach (var oldCommand in sender.TopLevelItems)
                    {
                        if (oldCommand == item)
                        {
                            toRemove.Add(item);
                            break;
                        }
                    }

                    foreach (var oldCommand in sender.FallbackItems)
                    {
                        if (oldCommand == item)
                        {
                            toRemove.Add(item);
                            break;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        foreach (var i in toRemove)
        {
            clone.Remove(i);
        }

        await sender.LoadTopLevelCommands();
        foreach (var i in sender.TopLevelItems)
        {
            clone.Add(new(new(i), false));
        }

        foreach (var i in sender.FallbackItems)
        {
            clone.Add(new(new(i), true));
        }

        ListHelpers.InPlaceUpdateList(TopLevelCommands, clone);
    }

    // Load commands from our extensions.
    // Currently, this
    // * queries the package catalog,
    // * starts all the extensions,
    // * then fetches the top-level commands from them.
    // TODO In the future, we'll probably abstract some of this away, to have
    // separate extension tracking vs stub loading.
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
        }

        IsLoading = false;

        return true;
    }

    public TopLevelCommandWrapper? LookupCommand(string id)
    {
        foreach (var command in TopLevelCommands)
        {
            if (command.Id == id)
            {
                return command;
            }
        }

        return null;
    }
}
