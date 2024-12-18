// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.CmdPal.Ext.System.Properties;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.System.Helpers;

public class SettingsManager
{
    private readonly string _filePath;
    private readonly Settings _settings = new();

    private static SettingsManager? instance;

    private readonly ToggleSetting _confirmSystemCommands = new(nameof(ConfirmSystemCommands), Resources.confirm_system_commands, string.Empty, false);
    private readonly ToggleSetting _showSuccessOnEmptyRB = new(nameof(ShowSuccessOnEmptyRB), Resources.Microsoft_plugin_sys_RecycleBin_ShowEmptySuccessMessage, string.Empty, false);
    private readonly ToggleSetting _localizeSystemCommands = new(nameof(LocalizeSystemCommands), Resources.Use_localized_system_commands, string.Empty, true);
    private readonly ToggleSetting _separateResultEmptyRB = new(nameof(SeparateResultEmptyRB), Resources.Microsoft_plugin_sys_RecycleBin_ShowEmptySeparate, string.Empty, false);
    private readonly ToggleSetting _reduceNetworkResultScore = new(nameof(ReduceNetworkResultScore), Resources.Reduce_Network_Result_Score, Resources.Reduce_Network_Result_Score_Description, true);

    public bool ConfirmSystemCommands => _confirmSystemCommands.Value;

    public bool ShowSuccessOnEmptyRB => _confirmSystemCommands.Value;

    public bool LocalizeSystemCommands => _localizeSystemCommands.Value;

    public bool SeparateResultEmptyRB => _separateResultEmptyRB.Value;

    public bool ReduceNetworkResultScore => _reduceNetworkResultScore.Value;

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    internal static string SettingsJsonPath()
    {
        // Get the path to our exe
        var path = global::System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the exe
        var directory = Path.GetDirectoryName(path) ?? string.Empty;

        // now, the state is just next to the exe
        return Path.Combine(directory, "system-state.json");
    }

    public SettingsManager()
    {
        _filePath = SettingsJsonPath();

        _settings.Add(_confirmSystemCommands);
        _settings.Add(_showSuccessOnEmptyRB);
        _settings.Add(_localizeSystemCommands);
        _settings.Add(_separateResultEmptyRB);
        _settings.Add(_reduceNetworkResultScore);

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

    public Settings GetSettings()
    {
        return _settings;
    }

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
}
