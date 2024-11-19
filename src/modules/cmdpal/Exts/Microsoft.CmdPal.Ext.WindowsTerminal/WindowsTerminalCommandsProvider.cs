// Copyright (c) Microsoft Corporation
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
    private readonly Settings _settings = new();
    private readonly TerminalTopLevelListItem _terminalCommand;
    private readonly SettingsManager _settingsManager;

    public const string ShowHiddenProfiles = nameof(ShowHiddenProfiles);
    public const string OpenNewTab = nameof(OpenNewTab);
    public const string OpenQuake = nameof(OpenQuake);

    public Settings TerminalSettings => _settings;

    public WindowsTerminalCommandsProvider()
    {
        DisplayName = $"Windows Terminal";

        var showHiddenProfiles = new ToggleSetting(ShowHiddenProfiles, "Show hidden profiles", "Show hidden profiles", false);
        var openNewTab = new ToggleSetting(OpenNewTab, "Open profiles in a new tab", "Open profiles in a new tab", false);
        var openQuake = new ToggleSetting(OpenQuake, "Open terminal in quake mode", "Open terminal in quake mode", false);

        _settings.Add(showHiddenProfiles);
        _settings.Add(openNewTab);
        _settings.Add(openQuake);
        _settingsManager = new(SettingsManager.SettingsJsonPath(), _settings);

        _terminalCommand = new TerminalTopLevelListItem(_settingsManager);
        _terminalCommand.MoreCommands = [new CommandContextItem(new SettingsPage(_settingsManager))];
    }

    public override IListItem[] TopLevelCommands()
    {
        return [_terminalCommand];
    }
}
