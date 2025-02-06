// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using TimeDateExtension.Helpers;

namespace TimeDateExtension;

internal sealed partial class TimeDateExtensionPage : DynamicListPage
{
    private readonly List<ListItem> _items = new();

    public TimeDateExtensionPage()
    {
        Icon = new(string.Empty);
        Name = "TimeDate extension for cmdpal";
        _items.Add(new ListItem(new CopyTextCommand(string.Empty)));
    }

    public override IListItem[] GetItems()
    {
        return _items.ToArray();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (!string.IsNullOrEmpty(newSearch))
        {
            try
            {
                var result = TimeDateCalculator.ExecuteSearch(newSearch);
                _items.Clear();
                _items.AddRange(result);
            }
            catch (Exception e)
            {
                _items.Clear();
                _items.Add(new ListItem(new CopyTextCommand(string.Empty)));
                _items[0].Title = e.Message;
            }

            if (_items.Count == 0)
            {
                _items.Add(new ListItem(new CopyTextCommand(string.Empty)));
                _items[0].Title = "Type an equation...";
            }
        }
        else
        {
            _items.Clear();
            _items.Add(new ListItem(new CopyTextCommand(string.Empty)));
            _items[0].Title = "Type an equation...";
        }
    }
}
