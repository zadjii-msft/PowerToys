// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace TimeDateExtension;

public partial class TimeDateExtensionActionsProvider : CommandProvider
{
    public TimeDateExtensionActionsProvider()
    {
        DisplayName = "TimeDate extension for cmdpal Commands";
    }

    private readonly ICommandItem[] _commands = [
        new CommandItem(new TimeDateExtensionPage()),
    ];

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
