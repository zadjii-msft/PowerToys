// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CmdPal.Ext.System.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Media.Capture.Core;

namespace Microsoft.CmdPal.Ext.System.Pages;

internal sealed partial class SystemListPage : ListPage
{
    private readonly SettingsManager _settingsManager;

    public SystemListPage()
    {
        Icon = new("\uE756");
        Id = "com.microsoft.cmdpal.system";
        Name = "List all System commands";
        PlaceholderText = "test it out yo";
        _settingsManager = SettingsManager.Instance;
    }

    public List<ListItem> Query(string query)
    {
        var results = new List<ListItem>();
        CultureInfo culture = _settingsManager.LocalizeSystemCommands ? CultureInfo.CurrentUICulture : new CultureInfo("en-US");

        if (query == null)
        {
            return results;
        }

        // normal system commands are fast and can be returned immediately
        var systemCommands = Commands.GetSystemCommands(IsBootedInUefiMode, _separateEmptyRB, _confirmSystemCommands, _showSuccessOnEmptyRB, IconTheme, culture);
        foreach (var c in systemCommands)
        {
            var resultMatch = StringMatcher.FuzzySearch(query.Search, c.Title);
            if (resultMatch.Score > 0)
            {
                c.Score = resultMatch.Score;
                c.TitleHighlightData = resultMatch.MatchData;
                results.Add(c);
            }
            else if (c?.ContextData is SystemPluginContext contextData)
            {
                var searchTagMatch = StringMatcher.FuzzySearch(query.Search, contextData.SearchTag);
                if (searchTagMatch.Score > 0)
                {
                    c.Score = resultMatch.Score;
                    results.Add(c);
                }
            }
        }

        return results;
    }

    public override IListItem[] GetItems() => [new ListItem(new NoOpCommand())];
}
