// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Windows.Foundation;

namespace Microsoft.CmdPal.Extensions.Helpers;

internal interface ISettingsForm
{
    public string ToForm();
    public void Update(JsonObject payload);
    public Dictionary<string, object> ToDictionary();
    public string ToDataIdentifier();

}

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

public sealed class ToggleSetting : Setting<bool>
{
    private ToggleSetting() : base() { }

    public ToggleSetting(string key, bool defaultValue) : base(key, defaultValue) { }
    
    public ToggleSetting(string key, string label, string description, bool defaultValue) :
        base(key, label, description, defaultValue)
    {
    }
    
    public override Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "type", "Input.Toggle" },
            { "title", Label },
            { "id", Key },
            { "label", Description },
            { "value", JsonSerializer.Serialize(Value) },
            { "isRequired", IsRequired },
            { "errorMessage", ErrorMessage }
        };
    }

    public static ToggleSetting LoadFromJson(JsonObject jsonObject)
    {
        return new ToggleSetting() { Value = jsonObject["value"]?.GetValue<bool>() ?? false };
    }
    public override void Update(JsonObject payload)
    {
        // Adaptive cards returns boolean values as a string "true"/"false", cause of course. 
        var strFromJson = payload[Key]?.GetValue<string>() ?? string.Empty;
        var val = strFromJson switch { "true" => true, "false" => false, _ => false };
        Value = val;
    }
}

public class TextSetting : Setting<string>
{
    private TextSetting() : base() {
        Value = string.Empty;
    }

    public TextSetting(string key, string defaultValue) : base(key, defaultValue) { }

    public TextSetting(string key, string label, string description, string defaultValue) :
        base(key, label, description, defaultValue)
    {
    }

    public override Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "type", "Input.Text" },
            { "title", Label },
            { "id", Key },
            { "label", Description },
            { "value", Value ?? string.Empty },
            { "isRequired", IsRequired },
            { "errorMessage", ErrorMessage }
        };
    }
    public static TextSetting LoadFromJson(JsonObject jsonObject)
    {
        return new TextSetting() { Value = jsonObject["value"]?.GetValue<string>() ?? string.Empty };
    }
    public override void Update(JsonObject payload)
    {
        Value = payload[Key]?.GetValue<string>() ?? string.Empty;
    }
}

public sealed class Settings
{
    private readonly Dictionary<string, object> _settings = new();
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public event TypedEventHandler<object, Settings>? SettingsChanged;

    public void Add<T>(Setting<T> s) {
        _settings.Add(s.Key, s);
    }

    public T? GetSetting<T>(string key) {
        if (_settings[key] is Setting<T> s)
        {
            return s.Value;
        }
        return default(T);
    }

    public bool TryGetSetting<T>(string key, out T? val)
    {
        object? o;
        if (_settings.TryGetValue(key, out o))
        {
            if (o is Setting<T> s)
            {
                val = s.Value;
                return true;
            }

        }

        val = default;
        return false;
    }
    internal string ToFormJson()
    {
        var settings = _settings
            .Values
            .Where(s => s is ISettingsForm)
            .Select(s => s as ISettingsForm)
            .Where(s => s != null)
            .Select(s => s!);

        var bodies = string.Join(",", settings
            .Select(s => JsonSerializer.Serialize(s.ToDictionary(), _jsonSerializerOptions)));
        var datas = string.Join(",", settings
            .Select(s => s.ToDataIdentifier()));

        var json = $$"""
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.5",
  "body": [
      {{bodies}} 
  ],
  "actions": [
    {
      "type": "Action.Submit",
      "title": "Save",
      "data": {
        {{datas}}
      }
    }
  ]
}
""";
        return json;
    }
    public IForm[] ToForms() {
        return [new SettingsForm(this)];
    }
    public void Update(string data)
    {
        var formInput = JsonNode.Parse(data)?.AsObject();
        if (formInput == null)
        {
            return;
        }
        foreach (var key in _settings.Keys)
        {
            var value = _settings[key];
            if (value is ISettingsForm f)
            {
                f.Update(formInput);
            }
        }
    }

    internal void RaiseSettingsChanged()
    {
        var handlers = SettingsChanged;
        handlers?.Invoke(this, this);
    }
}

public partial class SettingsForm : Form
{
    private readonly Settings _settings;
    internal SettingsForm(Settings settings)
    {
        _settings = settings;
        Template = _settings.ToFormJson();
    }
    public override ICommandResult SubmitForm(string payload)
    {
        var formInput = JsonNode.Parse(payload)?.AsObject();
        if (formInput == null)
        {
            return CommandResult.KeepOpen();
        }

        _settings.Update(payload);
        _settings.RaiseSettingsChanged();

        return CommandResult.KeepOpen();
    }

}
