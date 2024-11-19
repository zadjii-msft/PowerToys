// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions.Helpers;

#nullable enable

namespace Microsoft.CmdPal.Ext.WindowsTerminal.Helpers;

public class SettingsManager
{
    private readonly string _filePath;
    private readonly Settings _settings = new();

    private readonly ToggleSetting _showHiddenProfiles = new(ShowHiddenProfiles, "Show hidden profiles", "Show hidden profiles", false);
    private readonly ToggleSetting _openNewTab = new(OpenNewTab, "Open profiles in a new tab", "Open profiles in a new tab", false);
    private readonly ToggleSetting _openQuake = new(OpenQuake, "Open terminal in quake mode", "Open terminal in quake mode", false);

    private readonly Dictionary<string, object> settingsDict = new();

    public const string ShowHiddenProfiles = nameof(ShowHiddenProfiles);
    public const string OpenNewTab = nameof(OpenNewTab);
    public const string OpenQuake = nameof(OpenQuake);

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    internal static string SettingsJsonPath()
    {
        // Get the path to our exe
        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the exe
        var directory = Path.GetDirectoryName(path) ?? string.Empty;

        // now, the state is just next to the exe
        return Path.Combine(directory, "state.json");
    }

    public SettingsManager()
    {
        _filePath = SettingsJsonPath();

        _settings.Add(_showHiddenProfiles);
        _settings.Add(_openNewTab);
        _settings.Add(_openQuake);

        settingsDict[ShowHiddenProfiles] = _showHiddenProfiles.ToDictionary();
        settingsDict[OpenNewTab] = _openNewTab.ToDictionary();
        settingsDict[OpenQuake] = _openQuake.ToDictionary();

        // Load settings from file upon initialization
        LoadSettings();
    }

    public Settings GetSettings()
    {
        return _settings;
    }

    public void SaveSettings()
    {
        try
        {
            settingsDict[ShowHiddenProfiles] = _showHiddenProfiles.ToDictionary();
            settingsDict[OpenNewTab] = _openNewTab.ToDictionary();
            settingsDict[OpenQuake] = _openQuake.ToDictionary();

            // Serialize the main dictionary to JSON and save it to the file
            var settingsJson = JsonSerializer.Serialize(settingsDict, _serializerOptions);
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

            // Parse the JSON content into a JsonObject
            if (JsonNode.Parse(jsonContent) is JsonObject savedSettings)
            {
                foreach (KeyValuePair<string, JsonNode?> keyValue in savedSettings)
                {
                    var key = keyValue.Key;

                    var updatePayload = new JsonObject
                    {
                        [key] = keyValue.Value?["value"]?.DeepClone(),
                    };

                    if (key == ShowHiddenProfiles)
                    {
                        _showHiddenProfiles.Update(updatePayload);
                    }
                    else if (key == OpenNewTab)
                    {
                        _openNewTab.Update(updatePayload);
                    }
                    else if (key == OpenQuake)
                    {
                        _openQuake.Update(updatePayload);
                    }
                }
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
