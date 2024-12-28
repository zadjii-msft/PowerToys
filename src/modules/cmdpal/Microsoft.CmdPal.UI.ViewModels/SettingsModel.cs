// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class SettingsModel(IServiceProvider _serviceProvider) : ObservableObject
{
    public string TestString { get; set; } = string.Empty;

    public void FooBar()
    {
        _ = _serviceProvider.GetService<TopLevelCommandManager>();
    }

    public IEnumerable<CommandProviderWrapper> GetCommandProviders()
    {
        var manager = _serviceProvider.GetService<TopLevelCommandManager>()!;
        var allProviders = manager.CommandProviders;
        return allProviders;
    }
}
