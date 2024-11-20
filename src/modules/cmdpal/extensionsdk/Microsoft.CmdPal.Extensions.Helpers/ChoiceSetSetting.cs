﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Microsoft.CmdPal.Extensions.Helpers;

public sealed class ChoiceSetSetting : Setting<string>
{
    public sealed class Choice
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        public Choice(string value)
        {
            Value = value;
            Title = value;
        }
    }

    public List<Choice> Choices { get; set; }

    private ChoiceSetSetting()
        : base()
    {
        Choices = new();
    }

    public ChoiceSetSetting(string key, List<Choice> choices, Choice defaultValue)
        : base(key, defaultValue.Value)
    {
        Choices = choices;
    }

    public ChoiceSetSetting(string key, string label, List<Choice> choices, string description, Choice defaultValue)
        : base(key, label, description, defaultValue.Value)
    {
        Choices = choices;
    }

    public override Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "type", "Input.ChoiceSet" },
            { "title", Label },
            { "id", Key },
            { "label", Description },
            { "choices", Choices },
            { "value", Value ?? string.Empty },
            { "isRequired", IsRequired },
            { "errorMessage", ErrorMessage },
        };
    }

    public static ChoiceSetSetting LoadFromJson(JsonObject jsonObject)
    {
        return new ChoiceSetSetting() { Value = jsonObject["value"]?.GetValue<string>() ?? string.Empty };
    }

    public override void Update(JsonObject payload)
    {
        // If the key doesn't exist in the payload, don't do anything
        if (payload[Key] != null)
        {
            Value = payload[Key]?.GetValue<string>();
        }
    }

    public override string ToState()
    {
        return $"\"{Key}\": {JsonSerializer.Serialize(Value)}";
    }
}
