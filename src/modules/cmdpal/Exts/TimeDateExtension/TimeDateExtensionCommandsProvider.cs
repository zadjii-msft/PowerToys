// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Text;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TimeDateExtension.Helpers;
using TimeDateExtension.Pages;
using TimeDateExtension.Properties;

namespace TimeDateExtension;

public partial class TimeDateExtensionActionsProvider : CommandProvider
{
    private readonly CommandItem _command;
    private readonly SettingsManager _settingsManager = new();
    private static readonly CompositeFormat MicrosoftPluginTimedatePluginDescription = System.Text.CompositeFormat.Parse(Properties.Resources.Microsoft_plugin_timedate_plugin_description);

    public TimeDateExtensionActionsProvider()
    {
        DisplayName = Resources.Microsoft_plugin_timedate_plugin_name;

        _command = new CommandItem(new TimeDateExtensionPage(_settingsManager))
        {
            Icon = new IconInfo("\uEC92"), // DateTime icon
            Title = Resources.Microsoft_plugin_timedate_plugin_name,
            Subtitle = GetTranslatedPluginDescription(),
            MoreCommands = [new CommandContextItem(new SettingsPage(_settingsManager))],
        };
    }

    private string GetTranslatedPluginDescription()
    {
        // The extra strings for the examples are required for correct translations.
        var timeExample = Resources.Microsoft_plugin_timedate_plugin_description_example_time + "::" + DateTime.Now.ToString("T", CultureInfo.CurrentCulture);
        var dayExample = Resources.Microsoft_plugin_timedate_plugin_description_example_day + "::" + DateTime.Now.ToString("d", CultureInfo.CurrentCulture);
        var calendarWeekExample = Resources.Microsoft_plugin_timedate_plugin_description_example_calendarWeek + "::" + DateTime.Now.ToString("d", CultureInfo.CurrentCulture);
        return string.Format(CultureInfo.CurrentCulture, MicrosoftPluginTimedatePluginDescription, Resources.Microsoft_plugin_timedate_plugin_description_example_day, dayExample, timeExample, calendarWeekExample);
    }

    public override ICommandItem[] TopLevelCommands() => [_command];
}
