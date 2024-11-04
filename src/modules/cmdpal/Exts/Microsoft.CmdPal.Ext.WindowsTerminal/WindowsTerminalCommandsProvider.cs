// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Versioning;
using Microsoft.CmdPal.Extensions;

namespace Microsoft.CmdPal.Ext.WindowsTerminal;

public partial class WindowsTerminalCommandsProvider : ICommandProvider
{
    public string DisplayName => $"Windows Terminal";

    private readonly TerminalTopLevelListItem terminalCommand = new();

    public static ToggleSetting OpenNewTab { get; set; } = new(
        "Open in new tab",
        "openNewTab",
        "Open new tabs when launching a profile",
        true,
        "Ruh Roh",
        false);

    public static ToggleSetting ShowHiddenProfiles { get; set; } = new(
        "Show hidden profiles",
        "showHiddenProfiles",
        "Show hidden profiles in the profile list",
        false,
        "Ruh Roh",
        false);

    public static ToggleSetting OpenQuake { get; set; } = new(
        "Open Quake mode",
        "openQuake",
        "Open profiles in Quake mode",
        false,
        "Ruh Roh",
        false);

    public WindowsTerminalCommandsProvider()
    {
    }

    public IconDataType Icon => new(string.Empty);

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() => throw new NotImplementedException();
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    public IListItem[] TopLevelCommands()
    {
<<<<<<< Updated upstream
        return [terminalCommand];
=======
        return [terminalCommand, new ListItem(new TestPage(OpenNewTab, ShowHiddenProfiles, OpenQuake)), new ListItem(new TestSettingsListPage())];
>>>>>>> Stashed changes
    }
}
