// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.CmdPal.Ext.TimeDate.Helpers;
using Microsoft.CmdPal.Ext.TimeDate.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.TimeDate.Pages;

internal sealed partial class TimeDateExtensionPage : DynamicListPage
{
    private readonly SettingsManager _settingsManager;

    public TimeDateExtensionPage(SettingsManager settingsManager)
    {
        Icon = new("\uEC92"); // DateTime icon
        Title = Resources.Microsoft_plugin_timedate_plugin_name;
        Name = Resources.Microsoft_plugin_timedate_main_page_name;
        Id = "com.microsoft.cmdpal.timedate";
        _settingsManager = settingsManager;
    }

    public override IListItem[] GetItems() => DoExecuteSearch(SearchText).ToArray();

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        DoExecuteSearch(newSearch);
        RaiseItemsChanged(0);
    }

    private List<ListItem> DoExecuteSearch(string query)
    {
        try
        {
            var result = TimeDateCalculator.ExecuteSearch(_settingsManager, query);
            return result;
        }
        catch (Exception)
        {
            // sometimes, user's input may not correct.
            // In most of the time, user may not have completed their input.
            // So, we need to clean the result.
            // But in that time, empty result may cause exception.
            // So, we add a prompt for user.
            var items = new List<ListItem>();
            items.Add(new ListItem(new NoOpCommand()));
            items[0].Title = "Type an equation...";
            return items;
        }
    }
}
