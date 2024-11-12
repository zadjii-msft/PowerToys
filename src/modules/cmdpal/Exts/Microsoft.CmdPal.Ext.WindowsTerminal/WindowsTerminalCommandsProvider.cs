// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Versioning;
using Microsoft.CmdPal.Ext.WindowsTerminal.Pages;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WindowsTerminal;

public partial class WindowsTerminalCommandsProvider : CommandProvider
{
    private readonly Settings _settings = new();
    private readonly ListItem _settingsCommand;
    public const string ShowHiddenProfiles = nameof(ShowHiddenProfiles);

    public Settings TerminalSettings => _settings;

    public WindowsTerminalCommandsProvider()
    {
        DisplayName = $"Windows Terminal";
        var showHiddenProfiles = new ToggleSetting(ShowHiddenProfiles, "Show hidden profiles", "Show hidden profiles", true);
        var textSetting = new TextSetting("whatever", "Text setting", "This is a text setting", "Default text");
        _settings.Add(showHiddenProfiles);
        _settings.Add(textSetting);
        _settingsCommand = new ListItem(new SettingsPage(TerminalSettings));
    }

    public override IListItem[] TopLevelCommands()
    {
        return [new TerminalTopLevelListItem(this), _settingsCommand];
    }
}
