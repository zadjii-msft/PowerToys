// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.CmdPal.Ext.TimeDate.Properties;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.TimeDate.Helpers;

public class SettingsManager
{
    private readonly string _filePath;
    private readonly Microsoft.CmdPal.Extensions.Helpers.Settings _settings = new();

    private static SettingsManager? instance;

    private readonly List<ChoiceSetSetting.Choice> _calendarFirstWeekRuleChoices =
    [
        new ChoiceSetSetting.Choice(Resources.Microsoft_plugin_timedate_Setting_UseSystemSetting, "-1"),
        new ChoiceSetSetting.Choice(Resources.Microsoft_plugin_timedate_SettingFirstWeekRule_FirstDay, "0"),
        new ChoiceSetSetting.Choice(Resources.Microsoft_plugin_timedate_SettingFirstWeekRule_FirstFullWeek, "1"),
        new ChoiceSetSetting.Choice(Resources.Microsoft_plugin_timedate_SettingFirstWeekRule_FirstFourDayWeek, "2"),
    ];

    private readonly List<ChoiceSetSetting.Choice> _firstDayofWeekChoices = GetSortedListForWeekDaySetting();

    private readonly ChoiceSetSetting _calendarFirstWeekRuleChoiceSet;
    private readonly ChoiceSetSetting _firstDayOfWeekChoiceSet;
    private readonly ToggleSetting _onlyDateTimeNowGlobal = new(nameof(OnlyDateTimeNowGlobal), Resources.Microsoft_plugin_timedate_SettingOnlyDateTimeNowGlobal, Resources.Microsoft_plugin_timedate_SettingOnlyDateTimeNowGlobal_Description, true);
    private readonly ToggleSetting _timeWithSeconds = new(nameof(TimeWithSeconds), Resources.Microsoft_plugin_timedate_SettingTimeWithSeconds, Resources.Microsoft_plugin_timedate_SettingTimeWithSeconds_Description, false);
    private readonly ToggleSetting _dateWithWeekday = new(nameof(DateWithWeekday), Resources.Microsoft_plugin_timedate_SettingDateWithWeekday, Resources.Microsoft_plugin_timedate_SettingDateWithWeekday_Description, false);

    public string CalendarFirstWeekRule => _calendarFirstWeekRuleChoiceSet.Value ?? string.Empty;

    public string FirstDayOfWeek => _firstDayOfWeekChoiceSet.Value ?? string.Empty;

    public bool OnlyDateTimeNowGlobal => _onlyDateTimeNowGlobal.Value;

    public bool TimeWithSeconds => _timeWithSeconds.Value;

    public bool DateWithWeekday => _dateWithWeekday.Value;

    internal static string SettingsJsonPath()
    {
        // Get the path to our exe
        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the exe
        var directory = Path.GetDirectoryName(path) ?? string.Empty;

        // now, the state is just next to the exe
        return Path.Combine(directory, "time-date-state.json");
    }

    public SettingsManager()
    {
        _filePath = SettingsJsonPath();

        _calendarFirstWeekRuleChoiceSet = new(nameof(_calendarFirstWeekRuleChoiceSet), Resources.Microsoft_plugin_timedate_SettingFirstWeekRule, Resources.Microsoft_plugin_timedate_SettingFirstWeekRule_Description, _calendarFirstWeekRuleChoices);
        _firstDayOfWeekChoiceSet = new(nameof(_firstDayOfWeekChoiceSet), Resources.Microsoft_plugin_timedate_SettingFirstDayOfWeek, Resources.Microsoft_plugin_timedate_SettingFirstDayOfWeek, _firstDayofWeekChoices);

        _settings.Add(_calendarFirstWeekRuleChoiceSet);
        _settings.Add(_firstDayOfWeekChoiceSet);
        _settings.Add(_onlyDateTimeNowGlobal);
        _settings.Add(_timeWithSeconds);
        _settings.Add(_dateWithWeekday);

        // Load settings from file upon initialization
        LoadSettings();
    }

    internal static SettingsManager Instance
    {
        get
        {
            instance ??= new SettingsManager();
            return instance;
        }
    }

    public Microsoft.CmdPal.Extensions.Helpers.Settings GetSettings() => _settings;

    public void SaveSettings()
    {
        try
        {
            // Serialize the main dictionary to JSON and save it to the file
            var settingsJson = _settings.ToJson();

            File.WriteAllText(_filePath, settingsJson);
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = ex.ToString() });
        }
    }

    public void LoadSettings()
    {
        if (!File.Exists(_filePath))
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = "The provided settings file does not exist" });
            return;
        }

        try
        {
            // Read the JSON content from the file
            var jsonContent = File.ReadAllText(_filePath);

            // Is it valid JSON?
            if (JsonNode.Parse(jsonContent) is JsonObject savedSettings)
            {
                _settings.Update(jsonContent);
            }
            else
            {
                ExtensionHost.LogMessage(new LogMessage() { Message = "Failed to parse settings file as JsonObject." });
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = ex.ToString() });
        }
    }

    private static List<ChoiceSetSetting.Choice> GetSortedListForWeekDaySetting()
    {
        // List (Sorted for first day is Sunday)
        var list = new List<ChoiceSetSetting.Choice>
            {
                new(Resources.Microsoft_plugin_timedate_Setting_UseSystemSetting, "-1"),
                new(Resources.Microsoft_plugin_timedate_SettingFirstDayOfWeek_Sunday, "0"),
                new(Resources.Microsoft_plugin_timedate_SettingFirstDayOfWeek_Monday, "1"),
                new(Resources.Microsoft_plugin_timedate_SettingFirstDayOfWeek_Tuesday, "2"),
                new(Resources.Microsoft_plugin_timedate_SettingFirstDayOfWeek_Wednesday, "3"),
                new(Resources.Microsoft_plugin_timedate_SettingFirstDayOfWeek_Thursday, "4"),
                new(Resources.Microsoft_plugin_timedate_SettingFirstDayOfWeek_Friday, "5"),
                new(Resources.Microsoft_plugin_timedate_SettingFirstDayOfWeek_Saturday, "6"),
            };

        // Order Rules
        var orderRuleSaturday = new string[] { "-1", "6", "0", "1", "2", "3", "4", "5" };
        var orderRuleMonday = new string[] { "-1", "1", "2", "3", "4", "5", "6", "0" };

        switch (DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
        {
            case DayOfWeek.Saturday:
                return [.. list.OrderBy(x => Array.IndexOf(orderRuleSaturday, x.Value))];
            case DayOfWeek.Monday:
                return [.. list.OrderBy(x => Array.IndexOf(orderRuleMonday, x.Value))];
            default:
                // DayOfWeek.Sunday
                return list;
        }
    }
}
