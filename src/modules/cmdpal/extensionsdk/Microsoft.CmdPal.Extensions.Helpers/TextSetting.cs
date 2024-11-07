﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;

namespace Microsoft.CmdPal.Extensions.Helpers;

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
