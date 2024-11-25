// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.WindowsTerminal.Helpers;
using Microsoft.CmdPal.Ext.WindowsTerminal.Pages;
using Microsoft.CmdPal.Ext.WindowsTerminal.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WindowsTerminal;

public partial class WindowsTerminalCommandsProvider : CommandProvider
{
    private readonly TerminalTopLevelListItem _terminalCommand;
    private readonly SettingsManager _settingsManager = new();

    public WindowsTerminalCommandsProvider()
    {
        DisplayName = Resources.extension_name;

        _terminalCommand = new TerminalTopLevelListItem(_settingsManager)
        {
            MoreCommands = [
            new CommandContextItem(new SettingsPage(_settingsManager)),
            new CommandContextItem(new NoOpCommand()) { Title = "I'm a placeholder" },
        ],
        };
    }

    public override ICommandItem[] TopLevelCommands() => [_terminalCommand];
}
