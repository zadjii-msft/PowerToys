// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.CmdPal.Ext.Shell.Helpers;
using Microsoft.CmdPal.Ext.Shell.Pages;
using Microsoft.CmdPal.Ext.Shell.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.Shell;

public partial class ShellCommandsProvider : CommandProvider
{
    private readonly CommandItem _shellPageItem;
    private readonly SettingsManager _settingsManager = new();

    public ShellCommandsProvider()
    {
        DisplayName = Resources.wox_plugin_cmd_plugin_name;
        _shellPageItem = new CommandItem(new ShellListPage(_settingsManager))
        {
            Title = Resources.shell_command_name,
            Subtitle = Resources.wox_plugin_cmd_plugin_description,
            MoreCommands = [new CommandContextItem(new SettingsPage(_settingsManager))],
        };
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return [_shellPageItem];
    }
}
