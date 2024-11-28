// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.CmdPal.Ext.WebSearch.Commands;
using Microsoft.CmdPal.Ext.WebSearch.Helpers;
using Microsoft.CmdPal.Ext.WebSearch.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;

namespace Microsoft.CmdPal.Ext.WebSearch.Pages;

internal sealed partial class WebSearchListPage : DynamicListPage
{
    private readonly string _iconPath = string.Empty;
    private readonly SettingsManager _settingsManager;
    private static readonly CompositeFormat PluginInBrowserName = System.Text.CompositeFormat.Parse(Properties.Resources.plugin_in_browser_name);
    private static readonly CompositeFormat PluginOpen = System.Text.CompositeFormat.Parse(Properties.Resources.plugin_open);
    private ListItem[] searchListItem;

    public WebSearchListPage(SettingsManager settingsManager)
    {
        Name = Resources.command_item_title;
        Title = Resources.command_item_title;
        PlaceholderText = Resources.plugin_description;
        Icon = new(string.Empty);
        searchListItem = [new(new NoOpCommand())
        {
            Title = Properties.Resources.plugin_description,
            Subtitle = string.Format(CultureInfo.CurrentCulture, PluginOpen, BrowserInfo.Name ?? BrowserInfo.MSEdgeName),
        }
        ];
        Id = "com.microsoft.cmdpal.websearch";
        _settingsManager = settingsManager;
    }

    public List<ListItem> Query(string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var results = new List<ListItem>();
        var arguments = "? ";

        // empty query
        if (string.IsNullOrEmpty(query))
        {
            results.Add(new ListItem(new OpenCommandInShell(arguments, _settingsManager))
            {
                Title = Properties.Resources.plugin_description,
                Subtitle = string.Format(CultureInfo.CurrentCulture, PluginInBrowserName, BrowserInfo.Name ?? BrowserInfo.MSEdgeName),
                Icon = new(_iconPath),
            });
            return results;
        }
        else
        {
            var searchTerm = query;
            var searchArgs = $"? {searchTerm}";
            var result = new ListItem(new OpenCommandInShell(searchArgs, _settingsManager))
            {
                Title = searchTerm,
                Subtitle = string.Format(CultureInfo.CurrentCulture, PluginOpen, BrowserInfo.Name ?? BrowserInfo.MSEdgeName),
                Icon = new(_iconPath),
            };
            results.Add(result);
        }

        return results;
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        searchListItem = [.. Query(newSearch)];
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems()
    {
        // Convert history items to ListItem objects
        var historyListItems = _settingsManager.LoadHistory();

        // Combine existing searchListItem array with new history items
        var combinedList = new List<IListItem>(searchListItem);
        combinedList.AddRange(historyListItems);

        return [.. combinedList];
    }
}
