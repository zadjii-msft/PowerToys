﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CmdPal.UI.Pages;
using Microsoft.CmdPal.UI.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class ShellViewModel(IServiceProvider _serviceProvider) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsLoaded { get; set; } = false;

    [RelayCommand]
    public async Task<bool> LoadAsync()
    {
        var tlcManager = _serviceProvider.GetService<TopLevelCommandManager>();
        await tlcManager!.LoadBuiltinsAsync();

        IsLoaded = true;

        // TODO: would want to hydrate this from our services provider in the View layer, need to think about construction here...
        var page = _serviceProvider.GetService<MainListPage>();
        WeakReferenceMessenger.Default.Send<NavigateToListMessage>(new(new(page!)));

        // After loading builitns, and starting navigation, kick off a thread to load extensions.
        tlcManager.LoadExtensionsCommand.Execute(null);
        _ = Task.Run(async () =>
        {
            await tlcManager.LoadExtensionsCommand.ExecutionTask!;
            if (tlcManager.LoadExtensionsCommand.ExecutionTask.Status != TaskStatus.RanToCompletion)
            {
                // TODO: Handle failure case
            }
        });

        return true;
    }
}
