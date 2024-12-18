// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.Common.Services;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class TopLevelCommandManager : ObservableObject,
    IRecipient<ReloadCommandsMessage>
{
    private readonly IServiceProvider _serviceProvider;

    private IEnumerable<ICommandProvider>? _builtInCommands;

    public TopLevelCommandManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        WeakReferenceMessenger.Default.Register<ReloadCommandsMessage>(this);
    }

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
    }

    public async Task ReloadAllCommandsAsync()
    {
        // Oh dear this may have reavealed the awful truth that we load
        // extensions on the UI thread.
        //
        // If we stick this in a BG task, then it explodes, because
        // TopLevelCommands is Observable, so adding things to it can only
        // happen on the UI thread. Yike.
        IsLoading = true;
        var extensionService = _serviceProvider.GetService<IExtensionService>()!;
        await extensionService.SignalStopExtensionsAsync();
        TopLevelCommands.Clear();
        await LoadBuiltinsAsync();
        await LoadExtensionsAsync();
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

    public void Receive(ReloadCommandsMessage message) =>
        ReloadAllCommandsAsync().ConfigureAwait(false);
}
