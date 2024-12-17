// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.CmdPal.Ext.TimeDate.Helpers;
using Microsoft.CmdPal.Ext.TimeDate.Properties;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace Microsoft.CmdPal.Ext.TimeDate.Pages;

internal sealed partial class TimeDateListPage : DynamicListPage
{
    public TimeDateListPage()
    {
        Title = Resources.Microsoft_plugin_timedate_plugin_name;
        Icon = new(string.Empty);
        Name = Resources.Microsoft_plugin_timedate_get_timestamps;
        Id = "com.microsoft.cmdpal.timedate";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch) => RaiseItemsChanged(0);

    private List<TimeDateListItem> Query(string query) => SearchController.ExecuteSearch(query);

    public override IListItem[] GetItems() => [.. Query(SearchText)];
}
