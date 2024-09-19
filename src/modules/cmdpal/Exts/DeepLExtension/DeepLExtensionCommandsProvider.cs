// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace DeepLExtension;

public partial class DeepLExtensionActionsProvider : ICommandProvider
{
    public string DisplayName => $"DeepL Extension";

    public IconDataType Icon => new(string.Empty);

    private readonly IListItem[] _commands = [
        new ListItem(new DeepLExtensionPage())
        {
           Title = "DeepL",
           Subtitle = "DeepL Translation Extension",
        }
    ];

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() => throw new NotImplementedException();
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    public IListItem[] TopLevelCommands()
    {
        return _commands;
    }
}
