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
    }

    public override IListItem[] GetItems()
    {
        return [
            new ListItem(new NoOpCommand()) { Title = "TODO: Implement your extension here" }
        ];
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (!string.IsNullOrEmpty(newSearch))
        {
            var result = TimeDateCalculator.ExecuteSearch(newSearch);
            _items.Clear();
            _items.AddRange(result);
        }
    }
}
