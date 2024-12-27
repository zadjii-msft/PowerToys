// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;

namespace Microsoft.CmdPal.Extensions.Helpers;

public abstract class JsonSettingsManager
{
    private readonly Settings _settings = new();

    public Settings Settings => _settings;

    public string FilePath { get; init; } = string.Empty;

    public Settings GetSettings()
    {
        return _settings;
    }

    public void LoadSettings()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            throw new InvalidOperationException($"You must set a valid {nameof(FilePath)} before calling {nameof(LoadSettings)}");
        }

        var filePath = FilePath;
        if (!File.Exists(filePath))
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = "The provided settings file does not exist" });
            return;
        }

        try
        {
            // Read the JSON content from the file
            var jsonContent = File.ReadAllText(filePath);

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

    public void SaveSettings()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            throw new InvalidOperationException($"You must set a valid {nameof(FilePath)} before calling {nameof(SaveSettings)}");
        }

        try
        {
            // Serialize the main dictionary to JSON and save it to the file
            var settingsJson = _settings.ToJson();

            File.WriteAllText(FilePath, settingsJson);
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = ex.ToString() });
        }
    }
}
