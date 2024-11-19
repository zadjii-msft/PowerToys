﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.CmdPal.Ext.WindowsTerminal.Helpers;
using Microsoft.CmdPal.Ext.WindowsTerminal.Pages;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WindowsTerminal;

public partial class WindowsTerminalCommandsProvider : CommandProvider
{
    private readonly TerminalTopLevelListItem _terminalCommand;
    private readonly SettingsManager _settingsManager = new();

    public WindowsTerminalCommandsProvider()
    {
        DisplayName = $"Windows Terminal Profiles";

        _terminalCommand = new TerminalTopLevelListItem(_settingsManager);
        _terminalCommand.MoreCommands = [new CommandContextItem(new SettingsPage(_settingsManager))];
    }

    public override IListItem[] TopLevelCommands()
    {
        return [_terminalCommand];
    }
}
