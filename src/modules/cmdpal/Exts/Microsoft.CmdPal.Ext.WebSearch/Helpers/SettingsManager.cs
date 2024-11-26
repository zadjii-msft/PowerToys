// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text.Json.Nodes;
using Microsoft.CmdPal.Ext.WebSearch.Properties;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WebSearch.Helpers;

public class SettingsManager
{
    private readonly string _filePath;
    private readonly Microsoft.CmdPal.Extensions.Helpers.Settings _settings = new();

    private readonly ToggleSetting _globalIfURI = new(nameof(GlobalIfURI), Resources.plugin_global_if_uri, Resources.plugin_global_if_uri, false);

    public bool GlobalIfURI => _globalIfURI.Value;

    internal static string SettingsJsonPath()
    {
        // Get the path to our exe
        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the exe
        var directory = Path.GetDirectoryName(path) ?? string.Empty;

        // now, the state is just next to the exe
        return Path.Combine(directory, "websearch_state.json");
    }

    public SettingsManager()
    {
        _filePath = SettingsJsonPath();

        _settings.Add(_globalIfURI);

        // Load settings from file upon initialization
        LoadSettings();
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
}
