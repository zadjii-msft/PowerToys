// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Extensions.Helpers;
public abstract class Setting
{
    public string Id { get; set; }
    public string Label { get; set; }
    public bool IsRequired { get; set; }
    public string ErrorMessage { get; set; }

    public string DefaultValue { get; set; }

    public Setting(string id, string label, bool isRequired, string errorMessage, string defaultValue)
    {
        Id = id;
        Label = label;
        IsRequired = isRequired;
        ErrorMessage = errorMessage;
        DefaultValue = defaultValue;
    }

    public abstract Dictionary<string, object> ToDictionary();

    public static Setting FromJson(JsonObject jsonObject)
    {
        var type = jsonObject["type"]?.ToString();
        var title = jsonObject["title"]?.ToString() ?? string.Empty;
        var id = jsonObject["id"]?.ToString() ?? string.Empty;
        var label = jsonObject["label"]?.ToString() ?? string.Empty;
        var isRequired = jsonObject["isRequired"]?.GetValue<bool>() ?? false;
        var errorMessage = jsonObject["errorMessage"]?.ToString() ?? string.Empty;

        return type switch
        {
            "Input.Text" => new TextSetting(id, label, isRequired, errorMessage, jsonObject["style"]?.ToString() ?? "text", jsonObject["value"]?.GetValue<string>() ?? ""),
            "Input.Toggle" => new ToggleSetting(title, id, label, isRequired, errorMessage, (jsonObject["value"]?.GetValue<string>() ?? "") == "true" ? true : false),
            _ => throw new InvalidOperationException($"Unknown setting type: {type}")
        };
    }
}
public class TextSetting : Setting
{
    public string Style { get; set; }

    public TextSetting(string id, string label, bool isRequired, string errorMessage, string style, string defaultValue = "")
        : base(id, label, isRequired, errorMessage, defaultValue)
    {
        Style = style;
        DefaultValue = defaultValue;
    }

    public override Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "type", "Input.Text" },
            { "style", Style },
            { "id", Id },
            { "label", Label },
            { "isRequired", IsRequired },
            { "errorMessage", ErrorMessage }
        };
    }
}

public class ToggleSetting : Setting
{
    public string Value { get; set; }
    public string Title { get; set; }

    public ToggleSetting(string title, string id, string label, bool isRequired, string errorMessage, bool defaultValue)
        : base(id, label, isRequired, errorMessage, defaultValue:defaultValue.ToString())
    {
        Value = defaultValue.ToString().ToLower(CultureInfo.CurrentCulture); 

        if (string.IsNullOrWhiteSpace(title))
        {
            Title = "";
        }
        else
        {
            Title = title;
        }
    }

    public override Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "type", "Input.Toggle" },
            { "title", Title },
            { "id", Id },
            { "label", Label },
            { "value", Value },
            { "isRequired", IsRequired },
            { "errorMessage", ErrorMessage }
        };
    }
}

public class InputNumberSetting : Setting
{

    public InputNumberSetting(string id, string label, bool isRequired, string errorMessage, int defaultValue)
        : base(id, label, isRequired, errorMessage, defaultValue: defaultValue.ToString(CultureInfo.InvariantCulture))
    {
    }

    public override Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "type", "Input.Number" },
            { "id", Id },
            { "value", int.Parse(DefaultValue, CultureInfo.InvariantCulture) },
            { "label", Label },
            { "isRequired", IsRequired },
            { "errorMessage", ErrorMessage }
        };
    }
}

public class SettingsManager
{
    private readonly List<Setting> _settings;
    private readonly List<Setting> _defaultSettings;
    private readonly string _settingsFilePath;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public SettingsManager(string settingsFilePath, List<Setting> defaultSettings)
    {
        _settingsFilePath = settingsFilePath;
        _defaultSettings = defaultSettings;
        _settings = new List<Setting>(_defaultSettings);
        LoadSettingsFromFile();
    }

    public List<Setting> GetSettings()
    {
        return _settings;
    }

    public string GenerateAdaptiveCardJson()
    {

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var bodyElements = _settings.Select(s => JsonSerializer.Serialize(s.ToDictionary(), options));

        var bodyJson = string.Join(",", bodyElements);
        var dataJson = string.Join(",", _settings.Select(s => $"\"{s.Id}\": \"{s.Id}\""));

        var json = $$"""
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.5",
  "body": [
      {{bodyJson}} 
  ],
  "actions": [
    {
      "type": "Action.Submit",
      "title": "Save",
      "data": {
        {{dataJson}}
      }
    }
  ]
}
""";


        return json;
    }
    public void LoadSettingsFromFile()
    {
        if (!File.Exists(_settingsFilePath))
        {
            SaveSettingsToFile();
        }

        var json = File.ReadAllText(_settingsFilePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            SaveSettingsToFile();
        }

        var settings = JsonSerializer.Deserialize<List<JsonObject>>(json);

        if (settings != null)
        {
            foreach (var settingObject in settings)
            {
                var setting = Setting.FromJson(settingObject);
                var existingSetting = _settings.FirstOrDefault(s => s.Id == setting.Id);
                if (existingSetting != null)
                {
                    _settings.Remove(existingSetting);
                }
                _settings.Add(setting);
            }
        }
        else
        {
            SaveSettingsToFile();
        }
    }
    public void UpdateSettings(Dictionary<string, string> updatedValues)
    {
        foreach (var kvp in updatedValues)
        {
            var setting = _settings.FirstOrDefault(s => s.Id == kvp.Key);
            if (setting != null)
            {
                if (setting is TextSetting textSetting)
                {
                    textSetting.Label = kvp.Value;
                }
                else if (setting is ToggleSetting toggleSetting)                 {
                    toggleSetting.Value = kvp.Value;
                }
            }
        }
        SaveSettingsToFile();
    }

    private void SaveSettingsToFile()
    {
        var settingsList = _settings.Select(s => s.ToDictionary()).ToList();
        var json = JsonSerializer.Serialize(settingsList, _jsonSerializerOptions);
        File.WriteAllText(_settingsFilePath, json);
    }
}

