// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TimeDateExtension.Helpers;

namespace TimeDateExtension.Pages;

internal sealed partial class TimeDateExtensionPage : DynamicListPage
{
    private readonly List<ListItem> _items = new();
    private SettingsManager _settingsManager;

    public TimeDateExtensionPage(SettingsManager settingsManager)
    {
        Icon = new(string.Empty);
        Name = "TimeDate";
        Id = "com.microsoft.cmdpal.timedate";
        _settingsManager = settingsManager;
        DoExecuteSearch(string.Empty);
    }

    public override IListItem[] GetItems()
    {
        return _items.ToArray();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        DoExecuteSearch(newSearch);
    }

    private void DoExecuteSearch(string query)
    {
        try
        {
            var result = TimeDateCalculator.ExecuteSearch(_settingsManager, query);
            _items.Clear();
            _items.AddRange(result);
            RaiseItemsChanged(0);
        }
        catch (Exception)
        {
            // sometimes, user's input may not correct.
            // In most of the time, user may not have completed their input.
            // So, we need to clean the result.
            // But in that time, empty result may cause exception.
            // So, we add a prompt for user.
            _items.Clear();
            _items.Add(new ListItem(new NoOpCommand()));
            _items[0].Title = "Type an equation...";
        }
    }
}
