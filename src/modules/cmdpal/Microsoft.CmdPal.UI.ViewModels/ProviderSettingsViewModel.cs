// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CmdPal.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.CmdPal.UI.ViewModels;

public partial class ProviderSettingsViewModel(CommandProviderWrapper _provider, ProviderSettings _providerSettings) : ObservableObject
{
    public string DisplayName => _provider.DisplayName;

    public string ExtensionName => _provider.Extension?.ExtensionDisplayName ?? "Built-in";

    public IconDataType Icon => _provider.Icon;

    public bool IsEnabled => _providerSettings.IsEnabled;
}
