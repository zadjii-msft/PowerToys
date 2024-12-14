// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.TimeDate.Pages;
using Microsoft.CmdPal.Ext.TimeDate.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.TimeDate;

public partial class TimeDateCommandsProvider : CommandProvider
{
    private readonly CommandItem _timeDatePageItem;

    public TimeDateCommandsProvider()
    {
        DisplayName = Resources.Microsoft_plugin_timedate_plugin_name;

        _timeDatePageItem = new CommandItem(new TimeDateListPage())
        {
            Icon = new("\uE756"),
            Title = Resources.Microsoft_plugin_timedate_plugin_name,
            Subtitle = Resources.Microsoft_plugin_timedate_plugin_description,
            MoreCommands = [new CommandContextItem(new SettingsPage())],
        };
    }

    public override ICommandItem[] TopLevelCommands() => [_timeDatePageItem];
}
