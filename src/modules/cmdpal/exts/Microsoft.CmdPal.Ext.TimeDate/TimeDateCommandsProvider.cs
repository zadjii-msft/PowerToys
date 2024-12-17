// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Text;
using Microsoft.CmdPal.Ext.TimeDate.Pages;
using Microsoft.CmdPal.Ext.TimeDate.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.TimeDate;

public partial class TimeDateCommandsProvider : CommandProvider
{
    private readonly CommandItem _timeDatePageItem;
    private static readonly CompositeFormat MicrosoftPluginTimedatePluginDescription = System.Text.CompositeFormat.Parse(Properties.Resources.Microsoft_plugin_timedate_plugin_description);

    public TimeDateCommandsProvider()
    {
        DisplayName = Resources.Microsoft_plugin_timedate_plugin_name;

        _timeDatePageItem = new CommandItem(new TimeDateListPage())
        {
            Icon = new("\uEC92"), // DateTime icon
            Title = Resources.Microsoft_plugin_timedate_plugin_name,
            Subtitle = GetTranslatedPluginDescription(),
            MoreCommands = [new CommandContextItem(new SettingsPage())],
        };
    }

    public string GetTranslatedPluginDescription()
    {
        // The extra strings for the examples are required for correct translations.
        var timeExample = Resources.Microsoft_plugin_timedate_plugin_description_example_time + "::" + DateTime.Now.ToString("T", CultureInfo.CurrentCulture);
        var dayExample = Resources.Microsoft_plugin_timedate_plugin_description_example_day + "::" + DateTime.Now.ToString("d", CultureInfo.CurrentCulture);
        var calendarWeekExample = Resources.Microsoft_plugin_timedate_plugin_description_example_calendarWeek + "::" + DateTime.Now.ToString("d", CultureInfo.CurrentCulture);
        return string.Format(CultureInfo.CurrentCulture, MicrosoftPluginTimedatePluginDescription, Resources.Microsoft_plugin_timedate_plugin_description_example_day, dayExample, timeExample, calendarWeekExample);
    }

    public override ICommandItem[] TopLevelCommands() => [_timeDatePageItem];
}
