// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.System.Pages;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.System;

public partial class SystemCommandsProvider : CommandProvider
{
    private readonly CommandItem _systemPageItem;

    public SystemCommandsProvider()
    {
        DisplayName = "TEMP NAME SYSTEM STUFF";

        _systemPageItem = new CommandItem(new SystemListPage())
        {
            Icon = new("\uE756"),
            Title = "System Stuff",
            Subtitle = "System stuff subtitle",
            MoreCommands = [new CommandContextItem(new SettingsPage())],
        };
    }

    public override ICommandItem[] TopLevelCommands() => [_systemPageItem];
}
