﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WindowsTerminal;

public partial class WindowsTerminalCommandsProvider : CommandProvider
{
    private readonly TerminalTopLevelListItem terminalCommand = new();

    public WindowsTerminalCommandsProvider()
    {
        DisplayName = $"Windows Terminal";
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return [terminalCommand];
    }
}
