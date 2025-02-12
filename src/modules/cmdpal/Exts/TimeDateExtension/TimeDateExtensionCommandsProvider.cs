// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TimeDateExtension.Helpers;
using TimeDateExtension.Pages;

namespace TimeDateExtension;

public partial class TimeDateExtensionActionsProvider : CommandProvider
{
    private readonly SettingsManager _settingsManager = new();

    private readonly CommandItem _command;

    public TimeDateExtensionActionsProvider()
    {
        Id = "TimeDate";
        DisplayName = "TimeDate extension for cmdpal Commands";
        Icon = new IconInfo("\uE756");

        _command = new CommandItem(new TimeDateExtensionPage(_settingsManager))
        {
            MoreCommands = [new CommandContextItem(new SettingsPage(_settingsManager))],
        };
    }

    public override ICommandItem[] TopLevelCommands() => [_command];
}
