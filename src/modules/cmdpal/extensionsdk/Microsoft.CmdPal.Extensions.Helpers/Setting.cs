// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Microsoft.CmdPal.Extensions.Helpers;

public abstract class Setting<T> : ISettingsForm
{
    private readonly string _key;
    
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public T? Value { get; set; }
    public string Key => _key;
    public bool IsRequired { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    protected Setting() {
        Value = default;
        _key = string.Empty;
    }

    public Setting(string key, T defaultValue) {
        _key = key;
        Value = defaultValue;
    }

    public Setting(string key, string label, string description, T defaultValue) {
        _key = key;
        Value = defaultValue;
        Label = label;
        Description = description;
    }

    public abstract Dictionary<string, object> ToDictionary();
    public string ToDataIdentifier()
    {
        return $"\"{_key}\": \"{_key}\"";
    }

    public string ToForm()
    {
        var bodyJson = JsonSerializer.Serialize(ToDictionary(), _jsonSerializerOptions);
        var dataJson = $"\"{_key}\": \"{_key}\"";
        
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

    /*public dynamic FromJson(JsonObject jsonObject)
    {
        var type = jsonObject["type"]?.ToString();

        dynamic setting = type switch
        {
            "Input.Text" => TextSetting.LoadFromJson(jsonObject),
            "Input.Toggle" => ToggleSetting.LoadFromJson(jsonObject),
            _ => throw new InvalidOperationException($"Unknown setting type: {type}")
        };
        
        setting._key = jsonObject["id"]?.ToString() ?? string.Empty;
        setting.Label = jsonObject["title"]?.ToString() ?? string.Empty;
        setting.Description = jsonObject["label"]?.ToString() ?? string.Empty;
        setting.IsRequired = jsonObject["isRequired"]?.GetValue<bool>() ?? false;
        setting.ErrorMessage = jsonObject["errorMessage"]?.ToString() ?? string.Empty;

        return setting;
    }*/

    public abstract void Update(JsonObject payload);
}
