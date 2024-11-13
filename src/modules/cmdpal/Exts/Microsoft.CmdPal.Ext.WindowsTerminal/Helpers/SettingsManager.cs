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
    private readonly Settings _settings;

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public SettingsManager(string filePath, Settings settings)
    {
        _filePath = filePath;
        _settings = settings;

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
            var settingsDict = new Dictionary<string, object>();

            // Access all settings using the new GetAllSettings method
            var allSettings = _settings.GetAllSettings();

            foreach (var (key, setting) in allSettings)
            {
                // Call ToDictionary on each setting if the method exists
                var toDictionaryMethod = setting.GetType().GetMethod("ToDictionary");
                if (toDictionaryMethod != null)
                {
                    // Invoke ToDictionary on the setting and add the result to settingsDict
                    if (toDictionaryMethod.Invoke(setting, null) is Dictionary<string, object> settingDict)
                    {
                        settingsDict[key] = settingDict;
                    }
                }
                else
                {
                    // If ToDictionary is not available, save the raw value
                    settingsDict[key] = setting;
                }
            }

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
            var jsonContent = File.ReadAllText(_filePath);
            if (JsonNode.Parse(jsonContent) is JsonObject jsonObject)
            {
                _settings.Update(jsonObject.ToJsonString());
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = ex.ToString() });
        }
    }
}
