// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.CmdPal.UI.ViewModels;

public class ProviderSettings
{
    public string PackageFamilyName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public string ProviderDisplayName { get; set; } = string.Empty;

    public string ProviderId => $"{PackageFamilyName}/{ProviderDisplayName}";

    public bool IsBuiltin => string.IsNullOrEmpty(PackageFamilyName);

    public ProviderSettings(CommandProviderWrapper wrapper)
    {
        PackageFamilyName = wrapper.Extension?.PackageFamilyName ?? string.Empty;
        ProviderDisplayName = wrapper.DisplayName;
    }
}
