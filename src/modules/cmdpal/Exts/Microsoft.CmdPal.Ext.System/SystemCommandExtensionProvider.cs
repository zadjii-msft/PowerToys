// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.System.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.System;

public partial class SystemCommandExtensionProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    public static readonly SystemCommandPage Page = new();

    public SystemCommandExtensionProvider()
    {
        DisplayName = "Windows System commands";
        _commands = [
            new CommandItem(Page)
            {
                Title = DisplayName,
                Icon = new IconInfo("\uE72E"),
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
