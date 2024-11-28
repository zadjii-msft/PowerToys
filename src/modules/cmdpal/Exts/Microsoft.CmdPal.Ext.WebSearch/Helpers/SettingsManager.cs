// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CmdPal.Ext.WebSearch.Commands;
using Microsoft.CmdPal.Ext.WebSearch.Properties;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.WebSearch.Helpers;

public class SettingsManager
{
    private readonly string _filePath;
    private readonly string _historyPath;
    private readonly Microsoft.CmdPal.Extensions.Helpers.Settings _settings = new();

    private readonly ToggleSetting _globalIfURI = new(nameof(GlobalIfURI), Resources.plugin_global_if_uri, Resources.plugin_global_if_uri, false);
    private readonly ToggleSetting _showHistory = new(nameof(ShowHistory), Resources.plugin_show_history, Resources.plugin_show_history, true);

    public bool GlobalIfURI => _globalIfURI.Value;

    public bool ShowHistory => _showHistory.Value;

    internal static string SettingsJsonPath()
    {
        // Get the path to our exe
        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the exe
        var directory = Path.GetDirectoryName(path) ?? string.Empty;

        // now, the state is just next to the exe
        return Path.Combine(directory, "websearch_state.json");
    }

    internal static string HistoryStateJsonPath()
    {
        // Get the path to our exe
        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the exe
        var directory = Path.GetDirectoryName(path) ?? string.Empty;

        // now, the state is just next to the exe
        return Path.Combine(directory, "websearch_history.json");
    }

    public void SaveHistory(HistoryItem historyItem)
    {
        if (historyItem == null)
        {
            return;
        }

        try
        {
            List<HistoryItem> historyItems;

            // Check if the file exists and load existing history
            if (File.Exists(_filePath))
            {
                var existingContent = File.ReadAllText(_filePath);
                historyItems = JsonSerializer.Deserialize<List<HistoryItem>>(existingContent) ?? [];
            }
            else
            {
                historyItems = [];
            }

            // Add the new history item
            historyItems.Add(historyItem);

            // Serialize the updated list back to JSON and save it
            var historyJson = JsonSerializer.Serialize(historyItems);
            File.WriteAllText(_filePath, historyJson);
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = ex.ToString() });
        }
    }

    public List<ListItem> LoadHistory()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }

            // Read and deserialize JSON into a list of HistoryItem objects
            var fileContent = File.ReadAllText(_filePath);
            var historyItems = JsonSerializer.Deserialize<List<HistoryItem>>(fileContent) ?? [];

            // Convert each HistoryItem to a ListItem
            var listItems = new List<ListItem>();
            foreach (var historyItem in historyItems)
            {
                listItems.Add(new ListItem(new OpenCommandInShell($"? {historyItem.SearchString}", this))
                {
                    Title = historyItem.SearchString,
                    Subtitle = historyItem.Timestamp.ToString("g", CultureInfo.InvariantCulture), // Ensures consistent formatting
                });
            }

            return listItems;
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = ex.ToString() });
            return [];
        }
    }

    public SettingsManager()
    {
        _filePath = SettingsJsonPath();
        _historyPath = HistoryStateJsonPath();

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
